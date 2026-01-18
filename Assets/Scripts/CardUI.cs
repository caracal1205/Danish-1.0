using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CardUI : MonoBehaviour
{
    public Card card;
    public Button button;
    public Text label; // Texte pour debug / affichage UI
    private GameManager gameManager;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<Text>();
    }

    // Mise à jour pour la 3D : passe le GameManager et active le modèle 3D
    public void Setup(Card c, GameManager manager)
    {
        card = c;
        gameManager = manager;
    
    // We no longer need to find children because 'this' object IS the card
        if (label != null) 
        {
            label.text = c.ToString(); // Keep for debug if needed
            label.enabled = false;
        }

        if (button == null) button = GetComponent<Button>();
        if (button != null) button.interactable = true;
    }      
    
    public void DisableInteraction()
    {
        if (button != null) button.interactable = false;
    }

    private void Enable3DModel(Suit suit, Rank rank)
    {
        // *** Ceci suppose que votre CardPrefab est structuré comme ceci : ***
        // CardPrefab
        // |-> Card3DContainer (GameObject)
        //     |-> Clubs (GameObject)
        //         |-> Rank_Two (GameObject - Modèle 3D)
        //         |-> ...
        //     |-> Diamonds 
        //     |-> Hearts
        //     |-> Spades
        
        string suitName = suit.ToString(); 
        string rankName = rank.ToString(); 

        Transform cardContainer = transform.Find("Card3DContainer"); 
        
        if (cardContainer == null)
        {
            // IMPORTANT : Ajustez ce chemin si votre hiérarchie est différente
            // Ex: cardContainer = transform.Find("MonObjet3DGlobal");
            return;
        }

        // 1. Désactiver tous les modèles 3D pour s'assurer qu'un seul est actif
        foreach (Transform child in cardContainer)
        {
            // Désactiver les groupes de Suits
            child.gameObject.SetActive(false); 
        }

        // 2. Trouver et activer le modèle spécifique (Suit/Rank)
        Transform suitGroup = cardContainer.Find(suitName);
        if (suitGroup != null)
        {
            // Activer le groupe de Suits d'abord
            suitGroup.gameObject.SetActive(true); 
            
            // Chercher le modèle de Rank dans le groupe de Suit
            Transform rankModel = suitGroup.Find("Rank_" + rankName); 
            
            if (rankModel != null)
            {
                rankModel.gameObject.SetActive(true);
            }
        }
    }
}
