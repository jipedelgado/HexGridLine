using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControllerScript : MonoBehaviour {

    // static pour le passage des valeurs à la scene "main"
    static public float size;
    static public float initialNbPawns;
    static public float longueurChaine;
    static public float NbPionsAjoutés;

    private void Start() {
        //On utilise comme valeur par défaut la valeur par défaut des Sliders
        size = GameObject.Find("SliderSize").GetComponent<UnityEngine.UI.Slider>().value;
        initialNbPawns = GameObject.Find("SliderNbPawns").GetComponent<UnityEngine.UI.Slider>().value;
        NbPionsAjoutés = GameObject.Find("SliderNbNewPawns").GetComponent<UnityEngine.UI.Slider>().value;
        longueurChaine = GameObject.Find("SliderLongueurChaine").GetComponent<UnityEngine.UI.Slider>().value;
    }

    public void setSize( float s ) { size = s; }

    public void setinitialNbPawns( float s ) { initialNbPawns = s; }

    public void setlongueurChaine( float s ) { longueurChaine = s; }

    public void setNbPionsAjoutés( float s ) { NbPionsAjoutés = s; }

    public void GoToMainScene() {
        SceneManager.LoadScene("Main");
    }
}

