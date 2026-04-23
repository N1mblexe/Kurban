using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Drone
{
    public class DecisionState : AnimalBaseState<DroneAI>
    {
        public DecisionState(DroneAI machine) : base(machine) { }

        public override void OnEnter()
        {
            Storage destination;
            if ((machine.backpack.weight / machine.backpack.capacity) > .5f)
            {
                destination = machine.baseStorage;
            }
            else
            {
                destination = null;
                foreach (var storage in machine.storages)
                {
                    if (!storage.gameObject.activeInHierarchy || storage.getWeightRatio() == 0)
                        continue;

                    if (destination == null)
                    {
                        destination = storage;
                        continue;
                    }

                    if (destination.getWeightRatio() < storage.getWeightRatio())
                        destination = storage;
                }

                if (destination == null)
                    destination = machine.baseStorage;

            }
            machine.target = destination;
            machine.ChangeState(machine.goToTargetState);
        }
    }



    public class GoToTargetState : AnimalBaseState<DroneAI>
    {
        public GoToTargetState(DroneAI machine) : base(machine) { }

        private Vector3 lastPosition;
        private Vector3 lastVelocity;

        public Vector3 acceleration;
        public float accelerationMagnitude;

        private float leanFactor = 2f;
        private float maxLeanAngle = 45f;
        private float leanSmoothness = 6f;

        private float accelerationFilterSpeed = 10f; 
        private float velocityDeadzone = 0.05f;      

        public override void OnEnter()
        {
            float time = Vector3.Distance(machine.transform.position, machine.target.transform.position) / machine.speed;

            lastPosition = machine.transform.position;
            lastVelocity = Vector3.zero;
            acceleration = Vector3.zero; 

            machine.transform.DOMove(machine.target.transform.position + (Vector3.up * machine.pickUpHeight), time)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    machine.ChangeState(machine.interactWithStorageState);
                });
        }

        public override void OnUpdate()
        {
            CalculateAcceleration();
            ApplyLeaning();
        }

        private void CalculateAcceleration()
        {
            if (Time.deltaTime == 0) return;

            Vector3 currentVelocity = (machine.transform.position - lastPosition) / Time.deltaTime;

            if (currentVelocity.sqrMagnitude < velocityDeadzone * velocityDeadzone)
            {
                currentVelocity = Vector3.zero;
            }

            Vector3 rawAcceleration = (currentVelocity - lastVelocity) / Time.deltaTime;

            acceleration = Vector3.Lerp(acceleration, rawAcceleration, Time.deltaTime * accelerationFilterSpeed);

            accelerationMagnitude = acceleration.magnitude;

            lastPosition = machine.transform.position;
            lastVelocity = currentVelocity;
        }

        private void ApplyLeaning()
        {
            Vector3 localAccel = machine.transform.InverseTransformDirection(acceleration);

            float targetPitch = Mathf.Clamp(localAccel.z * leanFactor, -maxLeanAngle, maxLeanAngle);
            float targetRoll = Mathf.Clamp(-localAccel.x * leanFactor, -maxLeanAngle, maxLeanAngle);

            Quaternion targetRotation = Quaternion.Euler(targetPitch, machine.transform.eulerAngles.y, targetRoll);

            machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, targetRotation, Time.deltaTime * leanSmoothness);
        }

        public override void OnExit()
        {
            acceleration = lastPosition = lastVelocity = Vector3.zero;
            accelerationMagnitude = 0;

            Vector3 currentEuler = machine.transform.eulerAngles;
            machine.transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);
        }
    }

    public class InteractWithStorageState : AnimalBaseState<DroneAI>
    {
        public InteractWithStorageState(DroneAI machine) : base(machine) { }

        [SerializeField] private Vector3 punchAmount = new Vector3(-0.1f, -0.1f, -0.1f);
        [SerializeField] private int vibrato = 5;
        [SerializeField] private float elasticity = 0.5f;
        [SerializeField] private float animationTime = 1f;

        public override void OnEnter()
        {
            machine.target.HandleInteraction(machine.backpack, !machine.target.getPlaceStatus());
            machine.transform.DOPunchScale(punchAmount, animationTime, vibrato, elasticity).OnComplete(() =>
            {
                machine.ChangeState(machine.decisionState);
            });
        }
    }


}