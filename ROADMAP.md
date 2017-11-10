a# Roadmap

## Version 0.0 - Prototype basique

### Communication en dehors du jeu
- Des joueurs se connectant à un serveur
- Sélection d'une salle de jeu
- Création d'une salle de jeu
- Sélection d'une équipe
- Entrée dans la salle de jeu

### Communication interne au jeu
- Déplacement d'un personnage
- Barre de vie d'un personnage
- Tir d'un personnage (légère physique)
- Élaboration des cooldowns sur les rewinds
- Rewind de la totalité des personnages
- Affiliation des personnages suivant des équipes
- Génération du Terrain simple coté serveur
- Création du Terrain côté client
- Génération d'éléments de décor (qui seront récupérés potentiellement comme éléments destructibles)
- Mort d'un personnage
- Respawn du personnage
- Nombre limité de respawn de personnages par équipe
- Mort définitive d'une équipe (et fin de la partie)
- HUD Basique (barre de vie, viseur, cooldown, capacités)
- Zone de respawn (invicibilité : insensibilité aux dégâts, différentiation de la zone par rapport au terrain)

## Version 0.1 - Personnages, tirs et physique

- Dans le lobby, sélection d'un style de personnage
    - Différentiation au niveau du style in-game graphique
    - Différentiation au niveau de la vie du personnage
    - Différentiation au niveau des tirs du personnage
- Collision entre les personnages
- Prise en compte de la distance pour les dégats
- Headshot (!)
- Tir allié
- Dispersion des balles
- Chargeur, avec cooldown de rechargement
- Mode "visée" avec ralentissement du personnage mais meilleure précision
- Amélioration de la physique des balles
- Peaufinement de la capacité rewind

## Version 0.2 - Génération avancée du terrain

- Enrichissement des paramètres de génération
- Ajout de batiments, villes
- Types de map (style d'organisation)
- Ajout d'éléments de décor destructibles (arbres, maisons, toits, ...)
- Création d'éléments destructibles
- Génération d'éléments destructibles sur le terrain

## Version 0.3 - IA Basique

- IA basique qui ne fait que se déplacer et tirer sur les ennemis.
- Utilisation de la capacité rewind dans des cas stéréotypés (mort d'un allié, grosse perte de vie)
- Créer un comportement de groupe lors du déplacement, et du choix de la cible
- (?) Voir un système de maître-esclave avec des IAs qui se contentent de suivre les décisions, et une autre qui les prend.

## Version 0.4 - Système de craft

- Ajout de la possibilité de destruction suivant un certain mode, des éléments destructibles
- Destruction d'éléments destructibles à partir de tirs
- Loot du résultat de la destruction
- Élément de loot 
- Utilisation des éléments lootés pour créer un élément destructible
- Destruction de l'élément destructible crée
- Loot moindre en termes de composants de l'élément destructible crée détruit
- Non prise en compte de la physique pour la destruction des éléments (un toit qui vole)
- "Arme" utilisée pour la destruction d'éléménts de décors et d'éléments construits. Permet après utilisation de looter sur un élément détruit. Fait des dégats négligeables aux joueurs.
- Armes réalisent des dégats sur les éléments destructibles, mais ne permettent pas de loot leurs composants.

## Version 0.5 - IA Avancée

- Estimation des déplacements des joueurs
- Destruction et construction de terrain


## Version 0.5 - Gameplay avancé

- Nouvelles armes pour les joueurs (bombes)
- Ajout d'une physique des bâtiments

## Version 0.X - Divers - bonus

- Minimap du terrain
- Bruitages
- Éléments de soin sur le terrain