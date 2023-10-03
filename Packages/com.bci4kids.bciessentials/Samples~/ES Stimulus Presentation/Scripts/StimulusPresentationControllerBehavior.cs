using System.Collections;
using UnityEngine;
using System;
using BCIEssentials.Controllers;
using BCIEssentials.StimulusEffects;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BCIEssentials.ControllerBehaviors
{
    public class StimulusPresentationControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.TVEP;

        [SerializeField] private float setFreqFlash;
        [SerializeField] private float realFreqFlash;

        private int[] frames_on = new int[99];
        private int[] frame_count = new int[99];
        private float period;
        private int[] frame_off_count = new int[99];
        private int[] frame_on_count = new int[99];

        //start of emily stuff
        public Camera mainCam;

        public Text _displayText;
        public bool _recording;
        private bool _offMessages;
        private bool _restingState;
        private bool _open;
        private bool _closed;

        private string stimulusString = "";

        private Dictionary<int, string> orderDict = new Dictionary<int, string>();
        private ColorFlashEffect2.FlashOnColor  _colorChoice;


        protected override void Start()
        {
            base.Start();
            
            mainCam = Camera.main;
            mainCam.enabled = true;
        
            _displayText = GameObject.Find("TextToDisplay").GetComponent<Text>();

            //randomize order of stimulus presentation 
            Randomize();
        }

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frames_on[i] = 0;
                frame_count[i] = 0;
                period = targetFrameRate / setFreqFlash;
                frame_off_count[i] = (int)Math.Ceiling(period / 2);
                frame_on_count[i] = (int)Math.Floor(period / 2);
                realFreqFlash = (targetFrameRate / (float)(frame_off_count[i] + frame_on_count[i]));
            }
        }

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            while (StimulusRunning)
            {
                string freqString = "";
                string trainingString= "";
                string markerString=  "";
                
                if(!_offMessages)
                {
                    freqString = freqString + "," + realFreqFlash.ToString();
                    trainingString = (trainingIndex <= _selectableSPOs.Count) ? trainingIndex.ToString() : "-1";
                    

                    markerString = "tvep," + _selectableSPOs.Count.ToString() + "," + trainingString + "," +
                                        windowLength.ToString() + freqString + stimulusString;
                }

                if(_offMessages)
                {
                    markerString = "Stimulus Off";
                }

                if(_restingState && _open)
                {
                    markerString = "Resting state, eyes open";
                }
                if(_restingState && _closed)
                {
                    markerString = "Resting state, eyes closed";
                }

                marker.Write(markerString);

                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
                
            }
        }
        

        protected override IEnumerator OnStimulusRunBehavior()
        {
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frame_count[i]++;
                if (frames_on[i] == 1)
                {
                    if (frame_count[i] >= frame_on_count[i])
                    {
                        _selectableSPOs[i].StopStimulus();
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        _selectableSPOs[i].StartStimulus();
                        frames_on[i] = 1;
                        frame_count[i] = 0;
                    }
                }
            }

            yield return null;
        }

        protected override IEnumerator OnStimulusRunComplete()
        {
            foreach (var spo in _selectableSPOs)
            {
                if (spo != null)
                {
                    spo.StopStimulus();
                }
            }

            yield return null;
        }

        public void StopStimulusRun(int j, int l)
        {
            if (j == 2)
            {
                setFreqFlash = 16f;
                //need to call these methods so all of the appropriate flashing variables are updated 
                OnStimulusRunComplete();
                PopulateObjectList();
            }
            else if (j == 3)
            {
                setFreqFlash = 36f;
                OnStimulusRunComplete();
                PopulateObjectList();
            }
            else if (j == 6)
            {
                setFreqFlash = 9.6f;
                
                ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();

                if (spoEffect != null)
                {
                    if (l == 0)
                       SetMaterial(1);
                    else if (l == 1)
                        SetMaterial(2);
                    else if (l == 2)
                        SetMaterial(3);
                    else if (l == 3)
                        SetMaterial(4);
                    else if (l == 4)
                        SetMaterial(5);
                    else if (l == 5)
                        SetMaterial(6);

                    OnStimulusRunComplete();
                    PopulateObjectList();
                }
            }
        }
            

        protected override IEnumerator RunStimulus()
        {
            //setup variables for camera rotation 
            var _rotateAway = Vector3.zero;
            _rotateAway.y = 90f;

            var _rotateBack = Vector3.zero;
            _rotateBack.y = -90f;
            
                mainCam.transform.Rotate(_rotateAway);
                _restingState = true;
                _open = true;
                //1 minute eyes open Resting State 
                yield return new WaitForSecondsRealtime(6f); //60
                _open = false;
                _closed = true;
                //1 minute eyes closed Resting State 
                yield return new WaitForSecondsRealtime(6f); //60
                _restingState = false;
                _closed = false;
                mainCam.transform.Rotate(_rotateBack);

                //first object set to 9.6Hz
                setFreqFlash = 9.6f;
                OnStimulusRunComplete();
                PopulateObjectList();

                //set initial color and contrast
                ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();

                SetMaterial(0);
                stimulusString = ", "  + orderDict[0];

                //5 seconds count down before starting
                _offMessages = true;                    
                mainCam.transform.Rotate(_rotateBack);
                StartCoroutine(DisplayTextOnScreen("fromFive"));
                yield return new WaitForSecondsRealtime(5f);
                mainCam.transform.Rotate(_rotateAway);
                _offMessages = false;

                for(var l = 0 ; l < 7; l++)
                //this loops through all 7 stimuli  
                {
                    for(var k = 0; k < 3; k++)
                    {
                        //do this 3 times so each stimulus is played at all 3 frequencies 
                        //a full run through of this loop is one 'set'
                        for(var j = 0; j < 3; j++)
                        //do this for loop 3 times (12 seconds on 8 seconds off * 3)
                        //one full run through of the j for loop = all flashes/offs for one frequency
                        {
                            for(var i = 0; i < 144*12; i++) //(StimulusRunning)
                            //the number that i is less than is the amount of seconds to flash for 
                            //144 = 1 seconds (frame rate is 144 Hz) so 12 seconds = i < 144*12
                            {
                                yield return OnStimulusRunBehavior();
                            }
                                
                            //rotate the camera away from the stimuli objects when they are "off"
                            //this is primarily for the textured stimuli because their "off state" is the grey background square 
                            //and I don't want that on the screen during the pauses.
                            mainCam.transform.Rotate(_rotateAway);
                            _offMessages = true;

                            //control the 3 second countdown during every 8 seconds "off" 
                            //want it to play in every case except the very last stimulus of the set
                            //at this one, want to display the survey message and have a 20 second rest
                            //(controlled outside of the k,j,i loops)
                            yield return new WaitForSecondsRealtime(5f); //5
                            StartCoroutine(DisplayTextOnScreen("fromThree"));
                            yield return new WaitForSecondsRealtime(3f);     

                            //rotate the camera back to facing the stimulus objects 
                            mainCam.transform.Rotate(_rotateBack);
                            _offMessages = false;

                            if(k == 0)
                            //k == 0 means the frequency is currently 9.6 Hz, want to change it if j == 2
                            {
                                if(j == 2)
                                //j = 2 means this is the 3rd time the (on/off) loop is running 
                                //need to change the frequency
                                {
                                //when StopStimulusRun is called with j = 2, the frequency is set to 16 Hz
                                StopStimulusRun(j, 0);
                                }
                            }
                        
                            if(k == 1)
                            //k == 1 means the frequency is currently 16 Hz, want to change it to 36 Hz when j == 2
                            {
                                if(j == 2)
                                {
                                    //when StopStimulusRun is called with j = 3, the frequency is set to 36 Hz
                                    StopStimulusRun(j+1, 0);
                                }
                            }
                        }
                    }

                    mainCam.transform.Rotate(_rotateAway);

                    //wait 20 seconds between sets and display the countdown 
                    //the first call to StartCountDown displays a message to respond to the survey
                    //(immediately after the flashing stops). And then wait 15 seconds before starting 
                    //the 5 second countdown. The number supplied to the first StartCountDown call can be anything other than 3f and 5f 
                    _offMessages = true;
                    StartCoroutine(DisplayTextOnScreen("survey"));
                    yield return new WaitForSecondsRealtime(15f);  //15

                    if(l != 6)
                    {
                        StartCoroutine(DisplayTextOnScreen("fromFive"));
                        yield return new WaitForSecondsRealtime(4f);
                    }

                    //when StopStimulusRun is called with 6, the frequency is set to 9.6 and depending on the value of l,
                    //the stimulus contrast/texture is changed
                    StopStimulusRun(6, l); 
                    yield return new WaitForSecondsRealtime(1f);
                    mainCam.transform.Rotate(_rotateBack); 
                    _offMessages = false;
                        
                    if(l == 6)
                    {
                        mainCam.transform.Rotate(_rotateAway);
                        _offMessages = true;
                        yield return new WaitForSecondsRealtime(8f);
                        StartCoroutine(DisplayTextOnScreen("endOfStimuli"));
                        yield return new WaitForSecondsRealtime(2f);
                        _offMessages = false;

                        _restingState = true;
                        _open = true;
                        //1 minute eyes open Resting State 
                        yield return new WaitForSecondsRealtime(60f); //60
                        _open = false;
                        _closed = true;
                        //1 minutes eye closed Resting State 
                        yield return new WaitForSecondsRealtime(60f); //60
                        _restingState = false;
                        _closed = false;

                        StartCoroutine(DisplayTextOnScreen("endOfSession"));
                        StopCoroutineReference(ref _runStimulus);
                    }
                }
                    StopCoroutineReference(ref _runStimulus);
                    StopCoroutineReference(ref _sendMarkers);
        }



//////Helper Methods
        public IEnumerator DisplayTextOnScreen(string textOption)
        {
            if(textOption == "fromThree")
            {
                _displayText.text = "3";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "2";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "1";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "";
            }
            else if(textOption == "fromFive")
            {
                _displayText.text = "Stimulus presentation in...";
                yield return new WaitForSecondsRealtime(2.0f);
                _displayText.text = "3 seconds";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "2 seconds";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "1 second";
                yield return new WaitForSecondsRealtime(1.0f);
               _displayText.text = "";
            }
            else if(textOption == "end")
            {
                _displayText. text = "End of stimuli";
                yield return new WaitForSecondsRealtime(2.0f);
                _displayText.text = "";
            }
            else if(textOption == "endOfSession")
            {
                _displayText. text = "End";
                yield return new WaitForSecondsRealtime(2.0f);
            }
            else if(textOption == "survey")
            {
                _displayText.text = "Survey";
                yield return new WaitForSecondsRealtime(5.0f);
                _displayText.text = "";
            }
        } 
        private void SetMaterial(int key)
        {
            ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();
            if (orderDict.TryGetValue(key, out string material))
            {
                _colorChoice = ColorFlashEffect2.FlashOnColor.Grey;
                    
                if (material == "MinContrast")
                {
                    spoEffect.SetContrast(ColorFlashEffect2.ContrastLevel.Min,  _colorChoice);
                    stimulusString = ", MinContrast";
                }
                else if (material == "MaxContrast")
                {
                    spoEffect.SetContrast(ColorFlashEffect2.ContrastLevel.Max,  _colorChoice);
                    stimulusString = ", MaxContrast";
                }
                else if (material == "Worms")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Worms);
                    stimulusString = ", Worms";
                }
                else if (material == "Static")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Static);
                    stimulusString = ", Static";
                }
                else if (material == "Checkerboard")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Checkerboard);
                    stimulusString = ", Checkerboard";
                }
                else if (material == "Voronoi")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Voronoi);
                    stimulusString = ", Voronoi";
                }
                else if (material == "WoodGrain")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Wood);
                    stimulusString = ", WoodGrain";
                }
            }
        }

        private void Randomize()
        {
                orderDict.Add(0, "MaxContrast");
                orderDict.Add(1, "MinContrast");
                orderDict.Add(2, "Worms");
                orderDict.Add(3, "Voronoi"); 
                orderDict.Add(4, "WoodGrain");
                orderDict.Add(5, "Checkerboard");
                orderDict.Add(6, "Static");

                System.Random random = new System.Random();
                List<int> keys = new List<int>(orderDict.Keys);
                int num = keys.Count;

                while (num > 1)
                {
                    num--;
                    int k = random.Next(num + 1);
                    int temp = keys[k];
                    keys[k] = keys[num];
                    keys[num] = temp;
                }

                List<string> values = new List<string>(orderDict.Values);
                    
                int n = values.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    string temp = values[k];
                    values[k] = values[n];
                    values[n] = temp;
                }

                Dictionary<int, string> intDict = new Dictionary<int, string>();
                    
                for (int i = 0; i < keys.Count; i++)
                {
                    intDict.Add(keys[i], values[i]);
                }

                List<KeyValuePair<int, string>> keyValuePairs = new List<KeyValuePair<int, string>>(intDict);
                    
                int c = keyValuePairs.Count;
                while (c > 1)
                {
                    c--;
                    int k = random.Next(c + 1);
                    KeyValuePair<int, string> temp = keyValuePairs[k];
                    keyValuePairs[k] = keyValuePairs[c];
                    keyValuePairs[c] = temp;
                }

                Dictionary<int, string> randomDict = new Dictionary<int, string>();

                foreach (var k in keyValuePairs)
                    randomDict.Add(k.Key, k.Value);

                orderDict = new Dictionary<int, string>(randomDict);       
            }
            
    }
}

    

