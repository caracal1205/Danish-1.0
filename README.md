# Danish-1.0
meilleur jeu de carte ever


// by chat gpt jsp ce que ça vaut xd

<!DOCTYPE html><html lang="fr">
<head>
  <meta charset="UTF-8">
  <title>Jeu de cartes - Danish</title>
  <style>
    body { font-family: Arial, sans-serif; background: #1b1b1b; color: #f1f1f1; text-align: center; }
    .zone-jeu { display: flex; flex-direction: column; align-items: center; gap: 20px; margin-top: 20px; }
    .joueur, .adversaire { display: flex; flex-direction: column; align-items: center; gap: 5px; }
    .cartes { display: flex; gap: 5px; flex-wrap: wrap; justify-content: center; }
    .carte { border: 1px solid #999; background: #333; padding: 10px 15px; border-radius: 5px; cursor: pointer; }
    .pioche, .pile { margin: 10px; padding: 10px; border: 2px dashed #888; }
  </style>
</head>
<body>
  <h1>Jeu de cartes - Danish</h1>  <div class="zone-jeu">
    <div class="adversaire">
      <div>Cartes visibles adversaire</div>
      <div class="cartes" id="adversaire-visibles"></div>
      <div>Cartes cachées adversaire</div>
      <div class="cartes" id="adversaire-cachees"></div>
    </div><div class="tas">
  <div class="pioche" id="pioche">Pioche</div>
  <div class="pile" id="pile">Pile centrale</div>
</div>

<div class="joueur">
  <div>Cartes en main</div>
  <div class="cartes" id="main-joueur"></div>
  <div>Cartes visibles</div>
  <div class="cartes" id="joueur-visibles"></div>
  <div>Cartes cachées</div>
  <div class="cartes" id="joueur-cachees"></div>
</div>

  </div>  <script>
    // Exemples de cartes texte
    const exempleCartes = ["2♣", "3♦", "4♥", "6♠", "7♣", "8♦", "10♥", "A♠", "Q♣", "K♦"];

    function genererCartesAleatoires(nb) {
      let cartes = [];
      for (let i = 0; i < nb; i++) {
        const index = Math.floor(Math.random() * exempleCartes.length);
        cartes.push(exempleCartes[index]);
      }
      return cartes;
    }

    // Remplir les mains de test
    document.getElementById("main-joueur").innerHTML = genererCartesAleatoires(3).map(c => `<div class="carte">${c}</div>`).join("");
    document.getElementById("joueur-visibles").innerHTML = genererCartesAleatoires(3).map(c => `<div class="carte">${c}</div>`).join("");
    document.getElementById("joueur-cachees").innerHTML = ["❓", "❓", "❓"].map(c => `<div class="carte">${c}</div>`).join("");

    document.getElementById("adversaire-visibles").innerHTML = genererCartesAleatoires(3).map(c => `<div class="carte">${c}</div>`).join("");
    document.getElementById("adversaire-cachees").innerHTML = ["❓", "❓", "❓"].map(c => `<div class="carte">${c}</div>`).join("");

    document.getElementById("pioche").innerText = "Pioche (reste: 20)";
    document.getElementById("pile").innerText = "Dernière carte : 7♣";
  </script></body>
</html>
