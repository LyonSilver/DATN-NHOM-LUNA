using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace BackendPRO {
public class GameAuth : MonoBehaviour {
public string localUrl="https://localhost:7043", onlineUrl="https://game.example.com"; public bool online;
public string AccessToken {get;private set;} public string RefreshToken {get;private set;}
public event Action<string> Status;
[Serializable] class Credentials {public string email,password;}
[Serializable] class External {public string provider,idToken;}
[Serializable] class Refresh {public string refreshToken;}
[Serializable] class Tokens {public string accessToken,refreshToken;}
public void Register(string email,string password)=>StartCoroutine(Send("/auth/register",JsonUtility.ToJson(new Credentials{email=email,password=password}),true));
public void Login(string email,string password)=>StartCoroutine(Send("/auth/login",JsonUtility.ToJson(new Credentials{email=email,password=password}),true));
public void ExternalLogin(string provider,string idToken)=>StartCoroutine(Send("/auth/external",JsonUtility.ToJson(new External{provider=provider,idToken=idToken}),true));
public void Link(string provider,string idToken)=>StartCoroutine(Send("/auth/link",JsonUtility.ToJson(new External{provider=provider,idToken=idToken}),false));
public void Renew()=>StartCoroutine(Send("/auth/refresh",JsonUtility.ToJson(new Refresh{refreshToken=RefreshToken}),true));
public void Logout()=>StartCoroutine(Send("/auth/logout",JsonUtility.ToJson(new Refresh{refreshToken=RefreshToken}),false,true));
public void LoadPlayer()=>StartCoroutine(Send("/player/me",null,false));
IEnumerator Send(string path,string json,bool tokens,bool logout=false){using(var r=new UnityWebRequest((online?onlineUrl:localUrl).TrimEnd('/')+path,json==null?"GET":"POST")){r.downloadHandler=new DownloadHandlerBuffer();r.timeout=20;if(json!=null){r.uploadHandler=new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));r.SetRequestHeader("Content-Type","application/json");}if(!string.IsNullOrEmpty(AccessToken))r.SetRequestHeader("Authorization","Bearer "+AccessToken);yield return r.SendWebRequest();if(logout){AccessToken=RefreshToken=null;}if(r.result!=UnityWebRequest.Result.Success){Status?.Invoke("HTTP "+r.responseCode+" "+r.error);yield break;}if(tokens){var t=JsonUtility.FromJson<Tokens>(r.downloadHandler.text);AccessToken=t.accessToken;RefreshToken=t.refreshToken;Status?.Invoke("Authenticated");}else Status?.Invoke(path=="/player/me"?r.downloadHandler.text:"OK");}}
}}
