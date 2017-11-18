using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour {
    const string API_KEY = "http://api.openweathermap.org/data/2.5/weather?zip=32792,us&appid=5a9834eab8978959bcfebf888918a85e";
    public GameObject status = null;
    // Use this for initialization
    void Start () {
        StartCoroutine(PostFace());
        status = FindObjectOfType<TextMesh>().gameObject;
    }

    IEnumerator<object> PostFace()
    {
      
        WWW request = new WWW(API_KEY);

        WWW www = new WWW(API_KEY);
        yield return www;
        if (www.error == null)
        {
            JSONObject jObj = new JSONObject(www.text);
            JSONObject wether = jObj.GetField("weather");
            status.GetComponent<TextMesh>().text = wether.ToString(); 
        }
        else
        {
            Debug.Log("ERROR: " + www.error);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
