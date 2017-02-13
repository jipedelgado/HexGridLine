using UnityEngine;
using UnityEngine.UI;

public class SetText : MonoBehaviour {

    public float value;
    private Text textComponent;
    private Transform myParent;
    private UnityEngine.UI.Slider Slider;

	void Start () {
        // on va chercher la valeur du Slider parent ...
        myParent = this.gameObject.transform.parent;
        Slider = myParent.GetComponent<Slider>();
        value = Slider.value;

        // ... et on l'affiche
        textComponent = GetComponent<Text>();
        textComponent.text = ((int) (Mathf.Round(value))).ToString();
    }

    public void SetSliderValue( int sliderValue ) {
        textComponent.text = sliderValue.ToString();
    }

    public void SetSliderValue( float sliderValue ) {
        textComponent.text = Mathf.Round(sliderValue).ToString();
    }
}
