using System.Collections;
using UnityEngine;

// Base class for the Stimulus Presenting Objects (SPOs)

namespace BCIEssentials.StimulusObjects
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SPO : MonoBehaviour
    {
        public Color onColour; //Color during the 'flash' of the object.
        public Color offColour; //Color when not flashing of the object.

        // Whether or not to include in the Controller object, used to change which objects are selectable
        public bool Selectable = true;
        public int SelectablePoolIndex;

        //Use a boolean to indicate whether or not this SPO has a subset image
        [SerializeField] public bool hasImageChild = false;

        private MeshRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // Turn the stimulus on
        public virtual float StartStimulus()
        {
            //This is just for an object renderer (e.g. 3D object). Use <SpriteRenderer> for 2D
            if(_renderer != null && _renderer.material != null)
            {
                _renderer.material.color = onColour;
            }


            //Return time since stim
            return Time.time;
        }

        // Turn off/reset the SPO
        public virtual void StopStimulus()
        {
            //This is just for an object renderer (e.g. 3D object). Use <SpriteRenderer> for 2D
            if(_renderer != null && _renderer.material != null)
            {
                _renderer.material.color = offColour;
            }
        }

        // What to do on selection
        public virtual void Select()
        {
            // This is free form, do whatever you want on selection

            StartCoroutine(QuickFlash());

            // Reset
            StopStimulus();
        }

        // What to do when targeted for training selection
        public virtual void OnTrainTarget()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue,
                objectScale.z * scaleValue);
        }

        // What to do when untargeted
        public virtual void OffTrainTarget()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue,
                objectScale.z / scaleValue);
        }

        // Quick Flash
        public IEnumerator QuickFlash()
        {
            for (int i = 0; i < 3; i++)
            {
                StartStimulus();
                yield return new WaitForSecondsRealtime(0.2F);
                StopStimulus();
                yield return new WaitForSecondsRealtime(0.2F);
            }
        }

    }
}
