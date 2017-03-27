using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class ectScript : MonoBehaviour {

    public string JsonToString(string target, string s)
    {
        string[] newS = Regex.Split(target, s);

        return newS[1];
    }

    public Vector3 JsonToVector3(JSONObject target)
    {
        Vector3 result = new Vector3();
        result.x = float.Parse(JsonToString(target.GetField("x").ToString(), "\""));
        result.y = float.Parse(JsonToString(target.GetField("y").ToString(), "\""));
        result.z = float.Parse(JsonToString(target.GetField("z").ToString(), "\""));

        return result;
    }

    public float StringToFloat(string target)
    {
        string[] part = target.Split('.');
        if (part.Length == 2)
            if (part[1].Length > 4)
                return float.Parse(part[0] + "." + part[1].Substring(0, 3));

        return float.Parse(target);
    }

    public Quaternion JsonToQuaternion(JSONObject target)
    {
        Quaternion result = new Quaternion();
        result.x = float.Parse(JsonToString(target.GetField("x").ToString(), "\""));
        result.y = float.Parse(JsonToString(target.GetField("y").ToString(), "\""));
        result.z = float.Parse(JsonToString(target.GetField("z").ToString(), "\""));
        result.w = float.Parse(JsonToString(target.GetField("w").ToString(), "\""));
        
        return result;
    }

    public int StringToInteger(string target, string s)
    {
        int result = -1;
        int.TryParse(JsonToString(target, s), out result);

        return result;
    }
}
