
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechToText : MonoBehaviour {


    private Text hypo;
    private Text recog;
    // Use this for initialization

    private void DicationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.LogFormat("DictationResult: {0}", text);
        recog.text += text;
    }
    private void DictationRecognizer_DictationHypothesis(string text)
    {
        //do stuff
        Debug.LogFormat("Dicttion Hypthesis: {0}", text);
        hypo.text += text;
    }
    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        //do something
        if (cause != DictationCompletionCause.Complete)
            Debug.LogErrorFormat("Dictation completed unsuccessfully");
    }
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        //do something
        Debug.LogErrorFormat("Dictation error: {0}; Hresult = {1}.", error, hresult);
    }

    void Start () {
    DictationRecognizer dicRec = new DictationRecognizer();

       // dicRec.DictationResult += (text, confidence) =>
       //  {
       //      Debug.LogFormat("Dictation result: {0}", text);
       //      recog.text += text + "\n";
       //  };

        dicRec.DictationResult += DicationRecognizer_DictationResult;
        dicRec.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dicRec.DictationComplete += DictationRecognizer_DictationComplete;
        dicRec.DictationError += DictationRecognizer_DictationError;

        dicRec.Start();
        Debug.Log("Got started");
    }	
}
