import random

def create_deck():
    """Crée un jeu de 52 cartes."""
    suits = ['♥', '♦', '♣', '♠']
    ranks = ['2', '3', '4', '5', '6', '7', '8', '9', '10', 'J', 'Q', 'K', 'A']
    return [{'rank': rank, 'suit': suit} for suit in suits for rank in ranks]

def shuffle_deck(deck):
    """Mélange le jeu de cartes."""
    random.shuffle(deck)
    return deck

def deal_cards(deck, num_players, cards_per_player):
    """Distribue les cartes aux joueurs."""
    hands = [[] for _ in range(num_players)]
    for i in range(cards_per_player):
        for j in range(num_players):
            if deck:
                hands[j].append(deck.pop(0))
    return hands, deck

def card_value(card):
    """Retourne la valeur numérique d'une carte pour la comparaison."""
    rank_values = {'2': 2, '3': 3, '4': 4, '5': 5, '6': 6, '7': 7, '8': 8, '9': 9, '10': 10, 'J': 11, 'Q': 12, 'K': 13, 'A': 14}
    return rank_values[card['rank']]

def display_card(card):
    """Affiche une carte de manière lisible."""
    return f"{card['rank']}{card['suit']}"

def play_norwegian_whist():
    """Joue une partie de Bataille Norvégienne."""
    print("--- Bienvenue à la Bataille Norvégienne ! ---")
    print("Le but est de se débarrasser de toutes ses cartes en jouant des cartes de valeur égale ou supérieure à la précédente.")
    print("Les 2 sont des jokers et peuvent être joués sur n'importe quelle carte.")
    print("Les 7 changent la règle de 'plus haut ou égal' en 'plus bas ou égal'.")
    print("Les 10 font sauter le tour du joueur suivant.")
    print("Les As mettent fin au tas et le joueur qui l'a posé peut commencer un nouveau tas.")
    print("Quatre cartes identiques jouées consécutivement mettent fin au tas et le joueur peut commencer un nouveau tas.")
    print("-" * 40)

    num_players = 0
    while num_players < 2 or num_players > 4:
        try:
            num_players = int(input("Combien de joueurs (2-4) ? "))
            if num_players < 2 or num_players > 4:
                print("Veuillez entrer un nombre de joueurs entre 2 et 4.")
        except ValueError:
            print("Veuillez entrer un nombre valide.")

    deck = create_deck()
    deck = shuffle_deck(deck)

    cards_per_player = 7 if num_players == 2 else 5
    player_hands, remaining_deck = deal_cards(deck, num_players, cards_per_player)
    player_names = [f"Joueur {i+1}" for i in range(num_players)]

    current_player_index = 0
    discard_pile = []
    play_direction_normal = True # True pour plus haut ou égal, False pour plus bas ou égal
    skip_next_turn = False

    while any(player_hands):
        current_player_name = player_names[current_player_index]
        current_player_hand = player_hands[current_player_index]

        print(f"\n--- C'est le tour de {current_player_name} ---")
        print(f"Vos cartes : {[display_card(card) for card in current_player_hand]}")
        print(f"Cartes restantes dans le talon : {len(remaining_deck)}")
        print(f"Pile de défausse : {([display_card(c) for c in discard_pile[-3:]] + ['...']) if len(discard_pile) > 3 else [display_card(c) for c in discard_pile]}")

        if not discard_pile:
            print("Le tas est vide. Vous pouvez jouer n'importe quelle carte.")
        else:
            top_card = discard_pile[-1]
            print(f"Carte actuelle sur le tas : {display_card(top_card)}")
            if play_direction_normal:
                print("Règle : Jouez une carte de valeur égale ou **supérieure**.")
            else:
                print("Règle : Jouez une carte de valeur égale ou **inférieure**.")

        if skip_next_turn:
            print(f"{current_player_name} est sauté pour ce tour en raison d'un 10 précédent.")
            skip_next_turn = False
            current_player_index = (current_player_index + 1) % num_players
            continue

        valid_moves = []
        for i, card in enumerate(current_player_hand):
            if not discard_pile: # Si le tas est vide
                valid_moves.append(i)
            elif card['rank'] == '2': # Le 2 est un joker
                valid_moves.append(i)
            elif card['rank'] == 'A': # L'As peut être joué n'importe quand pour terminer le tas
                valid_moves.append(i)
            elif play_direction_normal:
                if card_value(card) >= card_value(top_card):
                    valid_moves.append(i)
            else: # Direction inversée (plus bas ou égal)
                if card_value(card) <= card_value(top_card):
                    valid_moves.append(i)

        if not valid_moves:
            print(f"{current_player_name} n'a pas de coup valide. Pioche une carte.")
            if remaining_deck:
                drawn_card = remaining_deck.pop(0)
                current_player_hand.append(drawn_card)
                print(f"Vous avez pioché : {display_card(drawn_card)}")
            else:
                print("Le talon est vide. Vous passez votre tour.")
            current_player_index = (current_player_index + 1) % num_players
            continue

        print("Cartes jouables (index) : ", [str(i) for i in valid_moves])
        print("Saisissez 'p' pour piocher une carte si vous ne voulez pas jouer (si le talon est vide, cela ne fera rien).")
        print("Saisissez 'quitter' pour arrêter la partie.")

        choice = input("Votre choix (index de la carte ou 'p' pour piocher) : ").lower()

        if choice == 'quitter':
            print("Partie terminée. Merci d'avoir joué !")
            break

        if choice == 'p':
            if remaining_deck:
                drawn_card = remaining_deck.pop(0)
                current_player_hand.append(drawn_card)
                print(f"Vous avez pioché : {display_card(drawn_card)}")
            else:
                print("Le talon est vide, vous ne pouvez pas piocher.")
            current_player_index = (current_player_index + 1) % num_players
            continue

        try:
            card_index = int(choice)
            if card_index in valid_moves:
                played_card = current_player_hand.pop(card_index)
                discard_pile.append(played_card)
                print(f"{current_player_name} a joué : {display_card(played_card)}")

                # Vérifier les règles spéciales
                if played_card['rank'] == '7':
                    play_direction_normal = not play_direction_normal
                    print("La règle de jeu a été inversée !")
                elif played_card['rank'] == '10':
                    skip_next_turn = True
                    print(f"Le prochain joueur ({player_names[(current_player_index + 1) % num_players]}) va être sauté !")
                elif played_card['rank'] == 'A':
                    print(f"L'As a été joué ! Le tas est nettoyé et {current_player_name} peut commencer un nouveau tas.")
                    discard_pile.clear() # Réinitialise le tas

                # Vérifier si quatre cartes identiques ont été jouées
                if len(discard_pile) >= 4 and all(c['rank'] == played_card['rank'] for c in discard_pile[-4:]):
                    print(f"Quatre {played_card['rank']} ont été joués ! Le tas est nettoyé et {current_player_name} peut commencer un nouveau tas.")
                    discard_pile.clear()

                if not current_player_hand:
                    print(f"\n--- {current_player_name} a gagné la partie ! ---")
                    break # Fin de la partie

                # Si un As a été joué, le joueur actuel rejoue
                if played_card['rank'] != 'A' and not (len(discard_pile) >= 4 and all(c['rank'] == played_card['rank'] for c in discard_pile[-4:])):
                    current_player_index = (current_player_index + 1) % num_players
                # Sinon, le joueur actuel rejoue s'il a vidé le tas avec 4 cartes identiques
                elif played_card['rank'] == 'A' or (len(discard_pile) >= 4 and all(c['rank'] == played_card['rank'] for c in discard_pile[-4:])):
                    pass # Le même joueur rejoue
            else:
                print("Choix invalide. Veuillez sélectionner un index de carte jouable ou 'p'.")
        except ValueError:
            print("Entrée invalide. Veuillez entrer l'index de la carte ou 'p'.")

    if any(player_hands) and choice != 'quitter': # Si la partie s'est terminée sans gagnant (talon vide)
        print("\n--- La partie est terminée. Personne n'a pu vider ses cartes. ---")

if __name__ == "__main__":
    play_norwegian_whist()