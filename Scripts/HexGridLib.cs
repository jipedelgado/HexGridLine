using UnityEngine;
using System;
using System.Collections.Generic;

public class HexGridLib : MonoBehaviour {

    struct Constants {
        public const double racineDeTrois = 1.73205081;
    }

    public struct Point {
        public double x;
        public double y;

        public Point( double x, double y ) {
            this.x = x;
            this.y = y;
        }
    }

    public struct Hex : IEquatable<Hex> {
        public readonly double q;
        public readonly double r;
        public readonly double s;

        public bool Equals( Hex other ) {
            return q == other.q && r == other.r;
        }

        // Quatre créateurs

        public Hex( int q, int r, int s ) {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public Hex( int q, int r ) {
            this.q = q;
            this.r = r;
            this.s = -q - r;
        }

        public Hex( double q, double r, double s ) {
            if (q + r + s != 0) {
                throw new System.ArgumentException("q + r + s != 0");
            }
            this = hexRound(q, r, s);
        }

        public Hex( double x, double y ) {
            // Crée le Hex qui contient le point (x,  y), détecté par la souris
            double qq = x * 2 / 3;
            double rr = (-x / 3 + Constants.racineDeTrois * y / 3);
            Hex h = hexRound(qq, rr, -qq - rr);
            this.q = h.q;
            this.r = h.r;
            this.s = -q - r;
        }

        static public Hex hexRound( double q, double r, double s ) {
            // appelé par Hex(double, double, double) et Hex(double, double)
            int qq = (int)Math.Round(q);
            int rr = (int)Math.Round(r);
            int ss = (int)Math.Round(s);

            double qDiff = Math.Abs(q - qq);
            double rDiff = Math.Abs(r - rr);
            double sDiff = Math.Abs(s - ss);

            if (qDiff > rDiff && qDiff > sDiff) {
                qq = -rr - ss;
            }
            else if (rDiff > sDiff) {
                rr = -qq - ss;
            }
            else {
                ss = -qq - rr;
            }
            return new Hex(qq, rr, ss);
        }

        public Point centre() {
            double x;
            double y;
            x = 3 * q / 2;
            y = Constants.racineDeTrois * (q / 2 + r);
            Point p = new Point(x, y);
            return p;
        }

        // L'ensemble des Hex comme un espace vectoriel sur Z
        static public Hex Add( Hex a, Hex b ) {
            return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
        }

        static public Hex Substract( Hex a, Hex b ) {
            return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
        }

        static public Hex Scale( Hex a, int k ) {
            return new Hex(a.q * k, a.r * k, a.s * k);
        }

        static public List<Hex> directions = new List<Hex> {
            // la liste des 6 Hex tangents aux cotés de this
            // resp. NE, N, NO, SO, S, SE ( maj du 11/02/2017)
            new Hex(1, 0, -1),
            new Hex(0, 1, -1),
            new Hex(-1, 1, 0),
            new Hex(-1, 0, 1),
            new Hex(0, -1, 1),
            new Hex(1, -1, 0),
        };

        static public List<Hex> diagonals = new List<Hex> {
            // la liste des 6 Hex situés à une distance 2 de this
            new Hex(2, -1, -1),
            new Hex(1, 1, -2),
            new Hex(-1, 2, -1),
            new Hex(-2, 1, 1),
            new Hex(-1, -1, 2),
            new Hex(-1, -2, 1),
        };

        static public Hex Direction( int direction ) {
            Hex DirHex = new Hex(0, 0);
            try {
                DirHex = directions[direction];
            }
            catch (Exception e) {
                Debug.Log(" Direction( direction ):  direction out of range {0,...5)" + "\n" + e);
                Application.Quit();
            }
            return DirHex;
        }

        public Hex neighbor( int direction ) {
            return Add(this, Direction(direction));
        }

        public List<Hex> neighborhood() {
            List<Hex> voisinage = new List<Hex>();
            for (int i = 0; i < 6; i++) {
                voisinage.Add(Hex.Add(this, directions[i]));
            }
            return voisinage;
        }

        public List<Hex> diagonalNeighborhood() {
            List<Hex> voisinage = new List<Hex>();
            for (int i = 0; i < 6; i++) {
                voisinage.Add(Hex.Add(this, diagonals[i]));
            }
            return voisinage;
        }
        public int distanceToOrigine() {
            // distance Manhattan
            return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
        }

        static public int distanceToOrigin( Hex h ) {
            // distance Manhattan
            return (int)((Math.Abs(h.q) + Math.Abs(h.r) + Math.Abs(h.s)) / 2);
        }

        static public int distance( Hex a, Hex b ) {
            // distance Manhattan
            return distanceToOrigin(Substract(a, b));
        }

        static public Hex HexLerp( Hex a, Hex b, double t ) {
            // retourne le Hex entre a et b, à une distance proportionnelle à t
            double Q = a.centre().x + (b.centre().x - a.centre().x) * t;
            double R = a.centre().y + (b.centre().y - a.centre().y) * t;
            return new Hex(Q, R);
        }

        static public List<Hex> HexLineDraw( Hex a, Hex b ) {
            // retourne la liste des Hex représentant la ligne [a, b]
            int N = distance(a, b);
            List<Hex> line = new List<Hex>();
            double step = 1.0 / Math.Max(N, 1);
            for (int i = 0; i <= N; i++) {
                line.Add(HexLerp(a, b, step * i));
            }
            return line;
        }

        static public List<Hex> Ring( Hex center, int radius ) {
            // retourne la couronne autour de this à une distance radius
            List<Hex> results = new List<Hex>();
            Hex h = new Hex();
            h = Add(center, Scale(Direction(4), radius));
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < radius; j++) {
                    results.Add(h);
                    h = h.neighbor(i);
                }
            return results;
        }

        static public List<Hex> Spiral( Hex center, int radius ) {
            // retourne la spirale autour de this, jusqu'à la distance radius
            List<Hex> results = new List<Hex>();
            results.Add(center);
            for (int i = 0; i < radius; i++) {
                results.AddRange(Ring(center, i));
            }
            return results;
        }

        static public Hex Rotate60( Hex center, Hex h ) {
            // tourne h de 60 degrés autour de center
            return new Hex(center.q - h.r, center.r - h.s, center.s - h.q);
        }

        public List<Hex> neighborhood( int N ) {
            // les voisins de this intérieurs à la couronne de rayon N, celle-ci comprise
            List<Hex> results = new List<Hex>();
            for (int i = -N; i <= N; i++)
                for (int j = Math.Max(-N, -N - i); j <= Math.Min(N, N - i); j++)
                    results.Add(Hex.Add(this, new Hex(i, j, -i - j)));
            return results;
        }

        static public List<Hex> neighborhood( Hex center, int N ) {
            // les voisins de center intérieurs à la couronne de rayon N, celle-ci comprise
            // version static de la précédente fonction
            List<Hex> results = new List<Hex>();
            for (int i = -N; i <= N; i++)
                for (int j = Math.Max(-N, -N - i); j <= Math.Min(N, N - i); j++)
                    results.Add(Hex.Add(center, new Hex(i, j, -i - j)));
            return results;
        }

        static public List<Hex> reacheableHex( Hex start, List<Hex> obstacles, int amplitude ) {
            // retourne la liste de tous les Hex accessibles, à une distance inférieure à amplitude
            // en contournant les obstacles
            List<Hex> visited = new List<Hex> { };
            visited.Add(start);
            List<List<Hex>> fringes = new List<List<Hex>>();
            fringes.Add(new List<Hex>());
            fringes[0].Add(start);
            for (int i = 1; i <= amplitude; i++) {
                fringes.Add(new List<Hex>());
                foreach (Hex h in fringes[i - 1]) {
                    for (int j = 0; j < 6; j++) {
                        Hex voisin = h.neighbor(j);
                        if (!visited.Contains(voisin) && !obstacles.Contains(voisin)) {
                            visited.Add(voisin);
                            fringes[i].Add(voisin);
                        }
                    }
                }
            }
            return visited;
        }

        static public List<Hex> visibleHex( Hex start, List<Hex> obstacles, int amplitude ) {
            // retourne la liste de tous les Hex visibles, à une distance inférieure à amplitude
            List<Hex> visible = new List<Hex> { };
            List<Hex> reacheable = reacheableHex(start, obstacles, amplitude);
            foreach (Hex hex in reacheable) {
                int N = distance(start, hex);
                double step = 1.0 / Math.Max(N, 1);
                int i = 0;
                bool stop = false;
                while (i <= N && !stop) {
                    Hex h = HexLerp(start, hex, step * i);
                    if (obstacles.Contains(h)) {
                        stop = true;
                    }
                    else {
                        Hex newHex = HexLerp(start, hex, step * i);
                        // pas de doublons dans visible !
                        if (!visible.Contains(newHex)) {
                            visible.Add(newHex);
                        }
                    }
                    i++;
                }
            }
            return visible;
        }

        // utilitaire d'élimination des doublons, non utilisé
        static public List<Hex> purgeList( List<Hex> liste ) {
            List<Hex> newListe = new List<Hex>();
            foreach (Hex hex in liste) {
                if (!newListe.Contains(hex)) {
                    newListe.Add(hex);
                }
            }
            return newListe;
        }

        public override int GetHashCode() {
            // fonction de hashage, bijection de ZxZ dans Z
            // credit = szudzik.com

            int Q = (int)(q >= 0 ? 2 * q : -2 * q - 1);
            int R = (int)(r >= 0 ? 2 * r : -2 * r - 1);
            int S = (Q >= R ? Q * Q + Q + R : R * R + Q) / 2;
            return (q < 0 && r < 0) || (q >= 0 && r >= 0) ? S : -S - 1;
        }

        public Dictionary<Hex, Hex> Graphe( Hex objectif, List<Hex> obstacles, int amplitude ) {
            /* cree un arbre de racine this, construit par zones concentriques croissantes
               représentées par fringes[], de type Breadth First Search tree
               en excluant les obstacles
             */
            Dictionary<Hex, Hex> Arcs = new Dictionary<Hex, Hex>();

            List<Hex> visited = new List<Hex>();
            visited.Add(this);

            List<Hex> path = new List<Hex>();
            path.Add(this);

            List<List<Hex>> fringes = new List<List<Hex>>();
            fringes.Add(new List<Hex>());
            fringes[0].Add(this);
            bool stop = false;
            int i = 0;
            while (i++ < amplitude && !stop) {
                fringes.Add(new List<Hex>());
                foreach (Hex h in fringes[i - 1]) {
                    for (int j = 0; j < 6; j++) {
                        Hex voisin = h.neighbor(j);
                        if (!visited.Contains(voisin) && !obstacles.Contains(voisin)) {
                            visited.Add(voisin);
                            fringes[i].Add(voisin);
                            Arcs.Add(voisin, h);
                        }
                        if (voisin.Equals(objectif)) {
                            stop = true;
                            break;
                        }
                    }
                }
            }
            return Arcs;
        }

        public List<Hex> pathTo( Hex objectif, Dictionary<Hex, Hex> graphe ) {
            // graphe selon la définition classique
            List<Hex> path = new List<Hex>();
            Hex current = objectif;
            while (!current.Equals(this)) {
                path.Add(current);
                current = graphe[current];
            }
            return path;
        }

        static public List<Hex> shortestPath( Hex hex1, Hex hex2, List<Hex> obstacles, int amplitude ) {
            List<Hex> chemin = new List<Hex>();
            Dictionary<Hex, Hex> graph = new Dictionary<Hex, Hex>();
            graph = hex1.Graphe(hex2, obstacles, amplitude);
            chemin = hex1.pathTo(hex2, graph);
            return chemin;
        }

        static public string toString( Dictionary<Hex, Hex> graphe ) {
            // pour debug
            string s = "";
            foreach (KeyValuePair<Hex, Hex> kvp in graphe) {
                s = s + kvp.Key.ToString() + "  -> " + kvp.Value.ToString() + "\n";
            }
            return s;
        }

         public string toString() {
            // pour debug
            string s = ("( " + q + " , " + r + " )");                 
            return s;
        }

        static public List<Hex> intersection( List<Hex> list1, List<Hex> list2 ) {
            List<Hex> both = new List<Hex>();
            foreach (Hex hex in list1) {
                if (list2.Contains(hex))
                    both.Add(hex);
            }
            return both;
        }

        public int neighbourCount( List<Hex> listeHex ) {
            int count = 0;
            foreach (Hex h in neighborhood())
                if (listeHex.Contains(h))
                    count++;
            return count;
        }

        static public GameObject Plot( GameObject o, Hex hex ) {
            // instancie un objet au centre de son Hex, appelé par Display
            HexGridLib.Point p = hex.centre();
            Vector3 vecteurPosition = new Vector3((float)p.x, 0.01f, (float)p.y);
            GameObject go = (GameObject)Instantiate(o, vecteurPosition, o.transform.rotation);
            return go;
        }

        static public GameObject Plot( GameObject o, Hex hex, Dictionary<Hex, GameObject> mapHexGameobject ) {
            // instancie un objet au centre de son Hex, appelé par Display
            // et met à jour mapHexGameobject
            HexGridLib.Point p = hex.centre();
            Vector3 vecteurPosition = new Vector3((float)p.x, 0.01f, (float)p.y);
            GameObject go = (GameObject)Instantiate(o, vecteurPosition, o.transform.rotation);

            if (!mapHexGameobject.ContainsKey(hex)) {
                mapHexGameobject.Add(hex, go);
            }
            return go;
        }

        static public void setPosition( GameObject o, Hex hex ) {
            HexGridLib.Point p = hex.centre();
            Vector3 vecteurPosition = new Vector3((float)p.x, 0.01f, (float)p.y);
            o.transform.position = vecteurPosition;
        }

        public string displayHashCode( int hauteur, int largeur )
        // pour debugger getHashCode
        {
            string s = "";
            for (int i = -hauteur; i < hauteur; i++) {
                for (int j = -largeur; j < largeur + 1; j++) {
                    Hex hex = new Hex(i, j, -i - j);
                    s = s + "\t" + hex.GetHashCode().ToString();
                }
                s = s + "\n";
            }
            return s;
        }

        static public void display( List<Hex> liste, GameObject objet ) {
            // pour chaque objet de la liste, instancie un objet en son centre

            if (liste != null) {
                foreach (Hex hex in liste) {
                    Plot(objet, hex);
                }
            }
            else {
                Debug.Log("function display : argument Liste vide");
            }
        }

        static public void display( List<Hex> liste, GameObject objet, Dictionary<Hex, GameObject> mapHexGameobject ) {
            // pour chaque objet de la liste, instancie un objet en son centre
            // et met à jour mapHexGameobject

            if (liste != null) {
                foreach (Hex hex in liste) {
                    Plot(objet, hex, mapHexGameobject);
                }
            }
            else {
                Debug.Log("function display : argument Liste vide");
            }
        }

        static public void purge( Dictionary<Hex, GameObject> Dico, List<Hex> liste ) {
            // retire de dico les éléments de liste
            if (liste != null) {
                foreach (Hex hex in liste) {
                    if (Dico.ContainsKey(hex)) {
                        GameObject go;
                        Dico.TryGetValue(hex, out go);
                        Destroy(go);
                        Dico.Remove(hex);
                    }
                }
            }
        }

        private Hex HexFromGameObject( GameObject go, Dictionary<Hex, GameObject> dico ) {
            Hex hexReturn = new Hex(0, 0);
            foreach (KeyValuePair<Hex, GameObject> kvp in dico) {
                try {
                    if (kvp.Value == go) {
                        hexReturn = kvp.Key;
                    }
                }
                catch (KeyNotFoundException) {
                    Debug.Log(" GameObject not found in dico");
                }
            }
            return hexReturn;
        }

        private GameObject GameObjectFromHex( Hex hex, Dictionary<Hex, GameObject> mapHexGameobject ) {
            GameObject go = new GameObject();
            try {
                go = mapHexGameobject[hex];
            }
            catch (KeyNotFoundException) {
                Debug.Log(" GameObject not found in mapHexGameobject");
            }
            return go;
        }

        private List<GameObject> fromListHex( List<Hex> listeHex, Dictionary<Hex, GameObject> mapHexGameobject ) {
            // retourne la liste des GameObjects associés aux Hex de listeHex dans mapHexGameobject
            List<GameObject> listGameObject = new List<GameObject>();
            foreach (Hex hex in listeHex) {
                listGameObject.Add(GameObjectFromHex(hex, mapHexGameobject));
            }
            return listGameObject;
        }

        static public void setColor( string tag, Color couleur )
         // change la couleur des children de tous les GameObjects taggés tag
         {
            GameObject[] go = GameObject.FindGameObjectsWithTag(tag);
            Component[] renderers;
            foreach (GameObject ob in go) {
                renderers = ob.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers) {
                    r.material.color = couleur;
                }
            }
        }

        static public Hex HexFromGameObject( GameObject go ) {
            Vector3 v = go.transform.position;
            return new Hex(v.x, v.z);
        }

        public GameObject Instanciate( GameObject prefab )
        // la fonction Unity.Instanciate retourne void
        // celle-ci retourne le Gameobject instancié à partir du prefab
        {
            return (GameObject)Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        }

        public static Vector3 vectorFromHex(Hex origine, Hex sommet ) {
            Vector3 vecteur = 
                new Vector3((float) sommet.centre().x - (float)origine.centre().x,
                0.0f, 
                (float)sommet.centre().y - (float)origine.centre().y
                );
            return vecteur;
        }
    }
}
