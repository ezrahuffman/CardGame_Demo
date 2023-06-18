using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayfabControls : MonoBehaviour
{
 // TODO: names are weird, make sure everything is name in a consistent way

    [SerializeField] GameObject signUpTab, loginTab, startPanel, hudGO;
    public TMP_Text username, userEmail, userPassword, userEmailLogin, userPasswordLogin, errorSignUp, errorLogin;
    string _encryptedPassword;
   
    public void SignUpTab()
    {
        Debug.Log("LoginTab()");


        signUpTab.SetActive(true);
        loginTab.SetActive(false);

        ClearErrorText();
    }

    private void ClearErrorText()
    {
        errorSignUp.text = "";
        errorLogin.text = "";
    }

    public void LoginTab()
    {
        Debug.Log("LoginTab()");

        signUpTab.SetActive(false);
        loginTab.SetActive(true);

        ClearErrorText();
    }

    string Encrypt(string pass)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(pass);
        bs = x.ComputeHash(bs);
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach (byte b in bs)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        return s.ToString();
    }

    public void SignUp()
    {
        string usernameString = username.text.Remove(username.text.Length - 1);
        string emailString = userEmail.text.Remove(userEmail.text.Length - 1);
        string passwordString = userPassword.text.Remove(userPassword.text.Length - 1);

        var registerRequest = new RegisterPlayFabUserRequest { Email = emailString, Password = Encrypt(passwordString), Username = usernameString };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, RegisterSuccess, RegisterFailure);
    }

    public void Login()
    {
        string emailString = userEmailLogin.text.Remove(userEmailLogin.text.Length - 1);
        string passwordString = userPasswordLogin.text.Remove(userPasswordLogin.text.Length - 1);

        var loginRequest = new LoginWithEmailAddressRequest { Email = emailString, Password = Encrypt(passwordString) };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, LoginSuccess, LoginFailure);
    }

    private void RegisterFailure(PlayFabError obj)
    {
        errorSignUp.text = obj.GenerateErrorReport();

    }

    private void LoginFailure(PlayFabError obj)
    {
        errorLogin.text = obj.GenerateErrorReport();

    }

    private void RegisterSuccess(RegisterPlayFabUserResult obj)
    {
        errorSignUp.text = "";
        ContinueToNextScene();
    }

    private void LoginSuccess(LoginResult obj)
    {
        errorLogin.text = "";
        ContinueToNextScene();
    }

    private void ContinueToNextScene()
    {
        Debug.Log("Continue To Next Scene");
    }
}
