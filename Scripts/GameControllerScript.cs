using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hex = HexGridLib.Hex;

public class GameControllerScript : MonoBehaviour {
    public GameObject tile;
    public int size = 5;
    public int initialNbPawns = 5;
    public int longueurChaine = 3;
    public int NbPionsAjoutés = 3;

    private Hex centre;
    private Hex FirstCourant;
    private Hex SecondCourant;
    private List<Hex> plateau;
    private List<Hex> Obstacles;
    private List<Hex> ShortestPath;
    private Dictionary<Hex, GameObject> mapHexGameObject;
    private List<Color> Colors;
    private Stack<Hex> pile;
    private List<Hex> Fronter;
    private List<List<Hex>> ListeDeLignes;
    private List<Hex> Pions;
    private int score;

    void Start() {

        //  récupération des valeurs de la scène Menu"
        size = (int)Mathf.Round(UIControllerScript.size);
        initialNbPawns = (int)Mathf.Round(UIControllerScript.initialNbPawns);
        longueurChaine = (int)Mathf.Round(UIControllerScript.longueurChaine);
        NbPionsAjoutés = (int)Mathf.Round(UIControllerScript.NbPionsAjoutés);

        // Positionnement et mise à l'échelle de la camera
        Camera camera = GameObject.Find("Main Camera").transform.GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 30, 0);
        camera.orthographicSize = 2 * size;

        // initialisation des privates
        plateau = new List<Hex>();
        pile = new Stack<Hex>();
        Obstacles = new List<Hex>();
        ShortestPath = new List<Hex>();
        ListeDeLignes = new List<List<Hex>>();
        Fronter = new List<Hex>();
        mapHexGameObject = new Dictionary<Hex, GameObject>();
        Pions = new List<Hex>();
        centre = new Hex(0, 0);

        // Affectations
        Colors = new List<Color> { Color.cyan, Color.blue, Color.gray, Color.black, Color.white, Color.magenta, Color.red, Color.grey };
        plateau = Hex.Spiral(centre, size);
        Fronter = Hex.Ring(centre, size + 1);
        foreach (Hex h in Fronter) {
            Obstacles.Add(h);
        }

        // construction et affichage des éléments du jeu
        Hex.display(plateau, tile, mapHexGameObject);
        Hex.setColor("case", Color.yellow);
        foreach (KeyValuePair<Hex, GameObject> kvp in mapHexGameObject) {
            kvp.Value.name = "case # " + kvp.Key.toString();
        }
        DisplayHex(initialNbPawns);
    }

    void Update() {

        // si click gauche, on empile les Hex détectés
        if (Input.GetMouseButtonDown(0)) {
            Hex HexTempo = getHexWithMouse();
            if (plateau.Contains(HexTempo)) {
                pile.Push(HexTempo);
                Text ZoneAffichage = GameObject.Find("Affichage").GetComponent<Text>();
                ZoneAffichage.text = "";

            }
            else {
                Text ZoneAffichage = GameObject.Find("Affichage").GetComponent<Text>();
                ZoneAffichage.text = "Click en dehors du tableau !!";
            }
        }

        // si Click-droit ...
        if (Input.GetMouseButtonDown(1)) {
            // efface ShortestPath
            if (ShortestPath.Count > 0) {
                SetColor(ShortestPath, Color.yellow);
                ShortestPath.Clear();

                // déplace les couleurs
                SetColor(SecondCourant, GetColor(FirstCourant));
                SetColor(FirstCourant, Color.yellow);

                // met Obstacles à jour
                Obstacles.Remove(FirstCourant);
                Obstacles.Add(SecondCourant);

                // met Pions à jour
                Pions.Remove(FirstCourant);
                Pions.Add(SecondCourant);
            }
            // on recherche les lignes conformes, cad plus longue que "longueurChaine
            Hex HexTempo = getHexWithMouse();
            if (plateau.Contains(HexTempo)) {
                Text ZoneAffichage = GameObject.Find("Affichage").GetComponent<Text>();
                ZoneAffichage.text = "";
                ListeDeLignes = SearchLine(HexTempo, longueurChaine);
            }
            else {
                Text ZoneAffichage = GameObject.Find("Affichage").GetComponent<Text>();
                ZoneAffichage.text = "Click en dehors du tableau !!";
            }

            // puis on les traite

            // si des lignes conformes sont trouvées, on les retire du plateau et des Pions en jeu
            if (ListeDeLignes.Count > 0) {
                foreach (List<Hex> ligne in ListeDeLignes) {
                    SetColor(ligne, Color.yellow);
                    foreach (Hex hex in ligne) {
                        score++;
                        if (Obstacles.Contains(hex)) {
                            Obstacles.Remove(hex);
                        }
                        if (Pions.Contains(hex)) {
                            Pions.Remove(hex);
                        }
                    }
                }
            }
            // sinon, on rajoute des pions, si c'est possible. 
            // sinon, affichage d'un message
            else {
                if (Pions.Count <= plateau.Count - NbPionsAjoutés) {
                    DisplayHex(NbPionsAjoutés);
                }
                else {
                    Text ZoneAffichage = GameObject.Find("Affichage").GetComponent<Text>();
                    ZoneAffichage.text = "plus de coup possible";
                }
            }
        }

        // affichage du score
        if (score > 0) {
            Text AffichageScore = GameObject.Find("AffichageScore").GetComponent<Text>();
            AffichageScore.text = "Score = " + score.ToString();
        }


        // si la pile contient deux éléments, on la vide puis on dessine le plus court chemin, en vert
        if (pile.Count > 1) {
            SecondCourant = pile.Pop();
            FirstCourant = pile.Pop();
            Color col1 = GetColor(FirstCourant);
            Color col2 = GetColor(SecondCourant);
            if (col1 != Color.yellow && col2 == Color.yellow) {
                ShortestPath = Hex.shortestPath(FirstCourant, SecondCourant, Obstacles, 40);
                foreach (Hex hex in ShortestPath) {
                    SetColor(hex, Color.green);
                }
                SetColor(SecondCourant, GetColor(FirstCourant));
            }
        }
    }

    // on parcours les trois direction NE, N, NO extraites de Hex.directions[]
    // dans les deux sens en collectant les Hex de même couleur que origine
    // puis les lignes plus longues que "longueur"
    private List<List<Hex>> SearchLine( Hex origine, int longueur ) {
        List<List<Hex>> lignesOK = new List<List<Hex>>();
        int indexDirection = 0;
        while (indexDirection < 3) {
            List<Hex> ligne = new List<Hex>();
            Hex direction = Hex.directions[indexDirection];
            ligne.Add(origine);
            int sens = 0;
            while (sens < 2) {
                bool encore = true;
                Hex courant = origine;
                Color couleurOrigine = GetColor(origine);
                while (encore) {
                    courant = Hex.Add(courant, direction);
                    if (!Pions.Contains(courant)) {
                        encore = false;
                    }
                    else {
                        if (GetColor(courant) == couleurOrigine) {
                            ligne.Add(courant);
                        }
                        else {
                            encore = false;
                        }
                    }
                }
                // on change de sens
                direction = Hex.Scale(direction, -1);
                sens++;
            }
            if (ligne.Count >= longueur) {
                lignesOK.Add(ligne);
            }
            indexDirection++;
        }
        return lignesOK;
    }

    // pour debug
    private void Print( List<Hex> liste ) {
        string s = " List<Hex> ";
        foreach (Hex h in liste) {
            s = s + " - " + h.toString();
        }
        Debug.Log(s + "\n");
    }

    private void Print( List<List<Hex>> liste ) {
        foreach (List<Hex> ligne in liste) {
            Print(ligne);
        }
    }

    private Hex getHexWithMouse() {
        Hex go = new Hex(0, 0);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            go = new Hex(hit.point.x, hit.point.z);
        }
        return go;
    }

    private Color GetColor( Hex hex ) {
        GameObject go = mapHexGameObject[hex];
        Renderer r = go.GetComponentInChildren<Renderer>();
        return r.material.color;
    }

    private void SetColor( Hex hex, Color col ) {
        GameObject go = mapHexGameObject[hex];
        Renderer r = go.GetComponentInChildren<Renderer>();
        r.material.color = col;
    }

    private void DisplayHex( int n ) {
        GameObject go = new GameObject();
        Hex h = new Hex();
        int k = 0;
        while (k < n) {
            int index = UnityEngine.Random.Range(0, plateau.Count);
            h = plateau[index];
            if (!Obstacles.Contains(h)) {
                Obstacles.Add(h);
                Pions.Add(h);
                go = mapHexGameObject[h];
                int colorIndex = UnityEngine.Random.Range(0, Colors.Count);
                Color color = Colors[colorIndex];
                SetColor(h, color);
                k++;
            }
        }
    }

    private List<Hex> KeysFromDictionary( Dictionary<Hex, GameObject> dico ) {
        List<Hex> liste = new List<Hex>();
        foreach (KeyValuePair<Hex, GameObject> kvp in mapHexGameObject) {
            liste.Add(kvp.Key);
        }
        return liste;
    }

    private void SetColor( List<Hex> liste, Color color ) {
        if (liste != null) {
            foreach (Hex hex in liste) {
                SetColor(hex, color);
            }
        }
    }

    public void Quitter() {
        Application.Quit();
    }

    public void NouveauJeu() {
        SceneManager.LoadScene("Menu");
    }

    public void Recommencer() {
        SceneManager.LoadScene("Main");
    }
}


