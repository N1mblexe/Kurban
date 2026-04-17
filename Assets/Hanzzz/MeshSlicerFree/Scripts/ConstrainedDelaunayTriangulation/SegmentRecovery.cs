#define CHECK_VERTEX_ON_EDGE // considerably slows triangulation, uncomment if previously crashed or not terminating
using System.Collections.Generic;
using UnityEngine;

namespace Hanzzz.MeshSlicerFree
{

public partial class ConstrainedDelaunayTriangulation
{
    private void SegmentRecovery(List<int> edges)
    {
        for(int i=1; i<edges.Count; i+=2)
        {
            int e0 = edges[i-1];
            int e1 = edges[i];
            if(e0 == e1)
            {
                continue;
            }

            #if CHECK_VERTEX_ON_EDGE
            // add e0->e1 to constraints only if no vertices lie on it
            for(int j=3; j <m_vertices.Count; j++)
            {
                if(e0 == j || e1 == j)
                {
                    continue;
                }
                if(0 == Orient2D(e0,j,e1))
                {
                    Point2D e0j = m_vertices[j]-m_vertices[e0];
                    Point2D je1 = m_vertices[e1]-m_vertices[j];
                    double d = Point2D.Dot(e0j,je1);
                    if(d > 0d && d<(m_vertices[e1]-m_vertices[e0]).SquaredMagnitude())
                    {
                        goto NEXT;
                    }
                }
            }
            #endif
            if(m_constraints.Contains((e0,e1)) || m_constraints.Contains((e1,e0)))
            {
                continue;
            }
            m_constraints.Add((e0,e1));
            #if CHECK_VERTEX_ON_EDGE
            NEXT:
            continue;
            #endif
        }

        foreach((int,int) constraint in m_constraints)
        {
            if(-1 != GetEdgeIncident(constraint.Item1,constraint.Item2).Item1)
            {
                continue;
            }
            m_intersectEdges.Clear();
            m_newEdges.Clear();

            {
                bool found = false;
                int t = m_verticesIncidentTriangles[constraint.Item1];
                int p1 = -1;
                int p2 = -1;
                while(!found)
                {
                    OrientTriangle(t, constraint.Item1);
                    p1 = m_triangles[3*t+1];
                    p2 = m_triangles[3*t+2];
                    if(Intersect(p1,p2, constraint.Item1, constraint.Item2))
                    {
                        m_intersectEdges.Add((p1,p2));
                        found = true;
                    }
                    else
                    {
                        t = GetNeighbor(constraint.Item1, p1, t);
                        if(-1 == t)
                        {
                            break;
                        }
                    }
                }
                if(!found)
                {
                    t = m_verticesIncidentTriangles[constraint.Item1];
                    while(!found)
                    {
                        OrientTriangle(t, constraint.Item1);
                        p1 = m_triangles[3*t+1];
                        p2 = m_triangles[3*t+2];
                        if(Intersect(p1,p2, constraint.Item1, constraint.Item2))
                        {
                            m_intersectEdges.Add((p1,p2));
                            found = true;
                        }
                        else
                        {
                            t = GetNeighbor(constraint.Item1, p2, t);
                            if(-1 == t)
                            {
                                throw new System.Exception();
                            }
                        }
                    }
                }

                t = GetNeighbor(p1,p2,t);
                OrientTriangle(t,p1,p2);

                // FIX BUG: guard the while(true) loop — if geometry is degenerate this
                // can spin forever. Limit iterations to a safe upper bound.
                int edgeWalkLimit = m_triangles.Count + 1;
                int edgeWalkIter  = 0;
                while(true)
                {
                    if(++edgeWalkIter > edgeWalkLimit)
                    {
                        Debug.LogWarning("[MeshSlicer] SegmentRecovery: edge-walk limit reached, skipping constraint. Mesh may have degenerate geometry near the cut plane.");
                        m_intersectEdges.Clear();
                        break;
                    }

                    int p0 = m_triangles[3*t+0];
                    p1 = m_triangles[3*t+1];
                    p2 = m_triangles[3*t+2];
                    if(p0 == constraint.Item2)
                    {
                        break;
                    }
                    if(Intersect(p0,p1,constraint.Item1, constraint.Item2))
                    {
                        m_intersectEdges.Add((p0,p1));
                        t = GetNeighbor(p0,p1,t);
                        OrientTriangle(t, p0, p1);
                    }
                    else if(Intersect(p0,p2,constraint.Item1, constraint.Item2))
                    {
                        m_intersectEdges.Add((p0,p2));
                        t = GetNeighbor(p0,p2,t);
                        OrientTriangle(t, p0, p2);
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
            }
            
            // FIX BUG 1: The primary freeze source.
            // When ALL edges left in m_intersectEdges form non-convex quads (due to
            // floating-point precision), none can be flipped and the loop rotates them
            // endlessly. We detect a full cycle with no progress and break out safely.
            {
                int noProgressLimit = m_intersectEdges.Count + 1; // full rotation = no progress
                int noProgressCount = 0;

                while(0 != m_intersectEdges.Count)
                {
                    int p1 = m_intersectEdges[0].Item1;
                    int p2 = m_intersectEdges[0].Item2;
                    (int, int) ts = GetEdgeIncident(p1,p2);
                    int t0 = ts.Item1;
                    int t1 = ts.Item2;

                    OrientTriangle(t0,p1,p2);
                    OrientTriangle(t1,p1,p2);
                    int p0 = m_triangles[3*t0+0];
                    p1 = m_triangles[3*t0+1];
                    p2 = m_triangles[3*t0+2];
                    int p3 = m_triangles[3*t1+0];

                    int o013 = Orient2D(p0,p1,p3);
                    int o023 = Orient2D(p0,p2,p3);
                    if(!((-1 == o013 && 1 == o023) || (1 == o013 && -1 == o023)))
                    {
                        // Edge cannot be flipped yet — move to back and track how many
                        // consecutive non-flippable edges we have seen.
                        m_intersectEdges.Add(m_intersectEdges[0]);
                        m_intersectEdges.RemoveAt(0);
                        noProgressCount++;

                        if(noProgressCount >= noProgressLimit)
                        {
                            // Completed a full cycle with zero flips: all remaining edges
                            // are stuck. This is caused by near-degenerate geometry.
                            // Discard them to avoid an infinite loop.
                            Debug.LogWarning($"[MeshSlicer] SegmentRecovery: {m_intersectEdges.Count} edge(s) could not be flipped (degenerate geometry). Discarding to prevent freeze.");
                            m_intersectEdges.Clear();
                        }
                        continue;
                    }

                    // A flip succeeded — reset the no-progress counter and update the limit.
                    noProgressCount = 0;
                    noProgressLimit = m_intersectEdges.Count; // recompute after list shrinks

                    m_intersectEdges.RemoveAt(0);
                    FlipDiagonal(t0,t1);

                    if(p0 == constraint.Item1 || p3 == constraint.Item2 || p3 == constraint.Item1 || p0 == constraint.Item2)
                    {
                        m_newEdges.Add((p0,p3));
                    }
                    else if(Intersect(p0,p3,constraint.Item1,constraint.Item2))
                    {
                        m_intersectEdges.Add((p0,p3));
                    }
                    else
                    {
                        m_newEdges.Add((p0, p3));
                    }
                }
            }

            // FIX BUG 2: The secondary freeze source.
            // In theory the Lawson-flip restore always terminates, but with floating-point
            // precision issues the same diagonal can keep appearing as an InCircle violator,
            // cycling forever. Cap iterations to a safe upper bound.
            {
                bool hasSwap;
                int swapLimit = m_newEdges.Count * m_newEdges.Count + 1;
                int swapIter  = 0;
                do
                {
                    hasSwap = false;
                    for(int i=0; i<m_newEdges.Count; i++)
                    {
                        int p1 = m_newEdges[i].Item1;
                        int p2 = m_newEdges[i].Item2;
                        (int, int) ts = GetEdgeIncident(p1,p2);
                        int t0 = ts.Item1;
                        int t1 = ts.Item2;

                        if((p1 == constraint.Item1 && p2 == constraint.Item2) || (p2 == constraint.Item1 && p1 == constraint.Item2))
                        {
                            continue;
                        }

                        OrientTriangle(t0,p1,p2);
                        OrientTriangle(t1,p1,p2);
                        int p0 = m_triangles[3*t0+0];
                        p1 = m_triangles[3*t0+1];
                        p2 = m_triangles[3*t0+2];
                        int p3 = m_triangles[3*t1+0];

                        if(1 != InCircle(p0,p1,p2,p3))
                        {
                            continue;
                        }

                        hasSwap = true;
                        FlipDiagonal(t0,t1);
                        m_newEdges[i] = (p0, p3);
                    }

                    if(++swapIter > swapLimit)
                    {
                        Debug.LogWarning("[MeshSlicer] SegmentRecovery: Delaunay restore loop hit iteration limit (degenerate geometry). Aborting restore pass.");
                        break;
                    }
                }
                while(hasSwap);
            }
        }
    }
}

}
