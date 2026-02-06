using System.Collections.Generic;
using Convai.Scripts.Services.TranscriptSystem;
using UnityEngine;

namespace Convai.Scripts.TranscriptUI.Filters
{
    public class ConvaiTranscriptFilterBase : MonoBehaviour
    {
        protected readonly List<ConvaiNPC> NPCInsideColliderList = new();
        private SphereCollider _sphereCollider;
        protected ConvaiTranscriptHandler TranscriptHandler;

        private void OnEnable()
        {
            TranscriptHandler = GetComponent<ConvaiTranscriptHandler>();
            if (TryGetComponent(out SphereCollider sphereCollider))
            {
                return;
            }

            _sphereCollider = gameObject.AddComponent<SphereCollider>();
            _sphereCollider.radius = 5f;
            _sphereCollider.isTrigger = true;

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, _sphereCollider.radius, Vector3.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.TryGetComponent(out ConvaiNPC convaiNPC))
                {
                    NPCInsideColliderList.Add(convaiNPC);
                }
            }
        }

        private void OnDisable()
        {
            if (_sphereCollider != null)
            {
                Destroy(_sphereCollider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ConvaiNPC convaiNPC))
            {
                NPCInsideColliderList.Add(convaiNPC);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ConvaiNPC convaiNPC))
            {
                TranscriptHandler.RemoveCharacterChatId(convaiNPC.CharacterID);
                NPCInsideColliderList.Remove(convaiNPC);
            }
        }
    }
}
