# FUTHARK'S ADVENTURE - Roadmap Proto (v3)

Base : GDD octobre 2026 (chapitre 10), revue le 26/11/2026.
Echeance : ~~slice fonctionnelle le 01/12/2026, produit fini le 15/12/2026.~~ Un bon proto jouable (palier 0), echeance dure le 15/12/2026.
Budget : ~10-15 h/semaine jusqu'au 22/11, ~30 h/semaine du 23/11 au 15/12.

Legende :
[ ] a faire | [x] fait
(T) test unitaire EditMode | (V) verification visuelle en Play mode | (D) decision a prendre
(+) "si le temps le permet" : hors noyau garanti, reporte sans regret

## Regles de travail

- Chaque phase produit quelque chose de JOUABLE avant de passer a la suivante.
- Regles du jeu en C# pur (Core) testees ; MonoBehaviours = colle fine ; donnees dans des ScriptableObjects.
- Communication logique -> affichage par evenements (pas de dependance directe).
- Point d'entree unique : scene Bootstrap + Composition Root (seul endroit qui fait les new).
- Les [A DEFINIR] du GDD sont tranches au moment ou ils deviennent necessaires, ou au playtest.
- Checkpoint : si J2 n'est pas atteint le 15/11, on revoit le perimetre.
- Checkpoint : si la Phase 5 n'est pas terminee le 04/12, on re-revoit le perimetre (piste : Items en usage direct, sans inventaire).
---

## Planning (revision)

| Palier | Contenu | Echeance |
|---|---|---|
| 0 - Proto | Ennemis, etages generes, progression, items, score, State Machine | 15/12 (dure) |
| 0.5 - Polish | Assets, son, retours visuels, brouillard, leaderboard, pause... | Si temps restant avant le 15/12 |
| 1 a 5 | Paliers du GDD 10.3 | Apres le proto |

| Phase | Bloc | Estimation | Dates visees |
|---|---|---|---|
| 4 | Ennemis | ~15 h | 26/11 - 30/11 |
| 5 | Generation | ~12 h | 01/12 - 03/12 |
| 6 | Progression et boss | ~8 h | 04/12 - 05/12 |
| 7 | Items | ~12 h | 06/12 - 08/12 |
| 8 | Score et State Machine | ~10 h | 09/12 - 11/12 |
| 9 | Build et playtests | ~15 h | 12/12 - 15/12 |
| | Marge | ~8 h | |

---

## Phase 1 - Setup du projet [termine]

- [x] Projet Unity 6, Git, packages, arborescence, Assembly Definitions FutharksAdventure.{Core, Game, Tests}
- [x] (T) Les tests EditMode se lancent
- [x] Scene Bootstrap + script Bootstrap (Composition Root minimal)

---

## Phase 2 - Grille, affichage et tour par tour (S1) -> J1

Objectif : un carre qui se deplace sur une grille, un tour a la fois, camera qui le suit.

### 2.1 Modele de grille (Core) [termine]
- [x] GridPosition, TileType, FloorGrid
- [x] (T) Tests verts

### 2.2 Vue de grille (Game) [termine]
- [x] GridView : dessine TOUTE la FloorGrid dans un Tilemap (placeholders carres de couleur)
- [x] Etage de test code en dur (en attendant la generation procedurale de la Phase 5)
- [x] Convention : 1 case = 1 unite Unity
- [x] Camera fixe cadrant l'etage de test (le suivi du joueur arrive en 2.4)
- [x] Hors grille : fond uni (case Empty)
- [x] (V) l’etage de test s’affiche en entier, correctement oriente

### 2.3 Systeme de tours (Core) [termine]
- [x] (D) Seance de conception : tours, actions, deplacement, occupation des cases (avant tout code)
- [x] Direction : enum 4 directions cardinales + conversion en decalage GridPosition
- [x] EventBus : instance injectee (pas statique), Subscribe / Unsubscribe / Publish generiques
- [x] Entity (abstraite, setter de Position internal ; convention : seule OccupancyMap la modifie) + Player (vide pour l'instant)
- [x] OccupancyMap : couche des occupants, seule a modifier la position des entites
- [x] GameAction (abstraite) : CanExecute() public, TryExecute() avec garde, Execute() protege
- [x] MoveAction : case praticable et libre, sinon refus sans tour ; publie EntityMoved
- [x] IntentResolver : traduit une intention en action (direction -> MoveAction en 2.3)
- [x] TurnManager : action acceptee = tour resolu, publie TurnResolved ; refus = rien ne se passe
- [x] (T) Direction, EventBus, OccupancyMap, MoveAction, IntentResolver, TurnManager

### 2.4 Input, affichage des entites, camera, Bootstrap [termine]
- [x] Input Actions asset - Action Map Gameplay : Move, RangedAttack
- [x] Lecture de l'input (clavier : fleches / ZQSD / WASD / pave num) ; la logique ne connait que des actions abstraites
- [x] EntityView : composant generique d'affichage d'une entite (joueur pour l'instant), deplacement instantane
- [x] Camera qui suit le joueur (cinemachine);
- [x] Bootstrap : cree la grille, le joueur, l'input, le TurnManager, les vues, et les relie
- [x] (V) Le carre se deplace case par case, bloque par les murs, la camera le suit

### 2.5 Review de fin de phase [termine]
- [x] Review globale de la structure, des scripts et des tests
- [x] (T) IntentResolverTests reactives ; asmdef Tests renomme FutharksAdventure.Tests
- [x] EventBus : double abonnement ignore, handler null refuse
- [x] MoveAction : invariant "position de depart" verifie dans CanExecute (exception = bug, pas regle de jeu)
- [x] Gardes null des constructeurs (MoveAction, IntentResolver, TurnManager) ; taille de FloorGrid > 0
- [x] Gardes des scripts Game : Bootstrap, GridView, TileSetConfig, EntityView, InputController
- [x] (T) Tests de gardes complementaires (EventBus, MoveAction, IntentResolver, TurnManager, FloorGrid, OccupancyMap, RoomCarver, Direction)
- [x] Conventions : `is null` en C# pur, `== null` pour UnityEngine.Object ; style NUnit Assert.That
- Reportes : input en Button (palier), conversion GridPosition -> monde (au prochain besoin),
  ratio de la Game view (Palier 0.5, Pixel Perfect Camera), repasse de proprete (Phase 9)

---

## Phase 3 - Le joueur (S2)

### 3.1 Stats et ressources [termine]
- [x] Creature : Entity (Core) : PV, PV max, VisionRange, TakeDamage, IsDead (deduit des PV)
- [x] Player : Creature : nourriture, hachettes, TryConsumeFood
- [x] (D) VisionRange porte par Creature : remplace detectionDistance des ennemis
- [x] PlayerConfig (SO) lu par Bootstrap : PV 17, nourriture 150, hachettes 3, VisionRange 6
- [x] Cout du tour : -1 nourriture ; a 0 : -1 PV par tour (le tour qui mange la derniere nourriture ne coute pas de PV)
- [x] Evenements FoodChanged (Current), HealthChanged (Creature, Amount), PlayerDied
- [x] Ordre : EntityMoved -> FoodChanged | HealthChanged -> PlayerDied -> TurnResolved
- [x] Garde : joueur mort -> SubmitMove refuse silencieusement
- [x] (T) Consommation, famine, mort par famine sans ennemi ; TestFactory.CreatePlayer pour les tests
- [x] (D) Techno UI : uGUI (Canvas + TextMeshPro)
- [x] (D) Orchestration du tour : etapes explicites dans TurnManager (action -> cout du tour -> TurnResolved)
- [x] HUD : squelette du HUD final ; PV, nourriture, hachettes rafraichis sur TurnResolved
- [x] (V) PV et nourriture evoluent a l'ecran a chaque tour, famine visible

### 3.2 Combat [termine]
- [x] (D) Ou vit la logique "degats + HealthChanged + PlayerDied" (aujourd'hui privee dans TurnManager) pour qu'AttackAction la reutilise
- [x] Attaque melee : MoveAction vers une case ennemie => AttackAction
- [x] Degats fixes, toujours touche (le test de touche est un ajout de palier)
- [x] ThrowAction + selection automatique de cible (regles GDD 4.5) :
      alignement 4 axes, portee = VisionRange (carre), ligne de visee non bloquee par un mur,
      pas de case adjacente, plus proche, egalite -> cible precedente, sinon moins de PV
      (D) egalite de PV aussi : premier de la liste
- [x] Aucune cible ou stock vide : action refusee, aucun tour, aucune hachette perdue
- [x] (D) Entree du TurnManager : une methode Submit par action ~~ou Submit(intent)~~
- [x] HUD : le compteur de hachettes (deja affiche) diminue au lancer
- [x] (V) Le stock de hachettes diminue a l'ecran, refus ~~visible~~ sans tour consomme
- [x] (T) Un test par regle du tableau 4.5

(L'ancienne 3.3 "Potion et inventaire" est fusionnee dans la Phase 7 - Items.)

---

## Phase 4 - Ennemis

### 4.1 Decisions
- [x] (D) Ordre du tour : action du joueur (couts preleves a l'usage) -> cout du tour -> ennemis dans l'ordre de la liste -> TurnResolved
- [x] (D) Joueur tue pendant la phase ennemie : les ennemis suivants finissent le tour en silence
      (AttackAction refuse une cible morte) ; un seul PlayerDied
- [x] (D) Deroule sequentiel garde pour le proto : un joueur qui fuit n'est plus adjacent quand le monstre joue
      (resolution simultanee reportee, voir paliers)
- [x] (D) Les ennemis utilisent les memes GameAction que le joueur (MoveAction, AttackAction)
- [x] (D) Deplacement : BFS sur les murs seulement (les autres creatures sont ignorees pour le calcul du chemin) ;
      case suivante occupee -> autre case voisine qui rapproche aussi ; si aucune : [a trancher]
- [x] (D) Liste des ennemis : List<Enemy> dans le TurnManager, passee au constructeur, construite en debut d'etage ;
      ennemis morts sautes pendant la boucle (IsDead), purges en fin de tour ; AddEnemy si besoin avec le generateur (Phase 5)
- [x] (D) Hasard : une instance de System.Random injectee des la Phase 4 (anticipation de la Phase 5)

### 4.2 IA des ennemis (Core) - feature/enemy-ai
- [x] WeightedRandom.TryPick (selection ponderee) : poids int, 0 valide, negatif = exception,
      liste vide ou total 0 = false
- [ ] BFS : premier pas vers une cible, seuls les murs bloquent
- [ ] IEnemyBrain (interface anticipee pour l'attaque a distance et le boss du palier 0.5) + cerveau Stroll/Chase ;
      (D) le joueur est-il une dependance du cerveau ou un parametre de Decide ?
- [ ] Etat recalcule a chaque tour : Chase si le joueur est dans le carre de rayon VisionRange (murs ignores), sinon Stroll
- [ ] Adjacent au joueur (4 directions) = attaque systematique
- [ ] Chase : pas du BFS ; avec la probabilite randomStepChance, pas aleatoire pondere a la place
      (lateral fort, recul faible, direction de poursuite exclue) pour pouvoir etre seme
- [ ] Stroll : tirage pondere parmi les 4 directions + rester sur place, malus vers LastPosition (anti aller-retour)
- [ ] Aucune option possible (total 0) -> RefusedAction, le monstre reste sur place
- [ ] (T) WeightedRandom (FakeRandom dans TestStatics), BFS, cerveau Stroll/Chase

### 4.3 Tour des ennemis (Core) - feature/enemy-turn
- [ ] Phase ennemie dans le TurnManager : 1 action par ennemi (deplacement OU attaque) ;
      conflit de case : le premier de la liste gagne
- [ ] (T) Detection, poursuite, attaque ; aucun ennemi ne traverse un mur ou un autre ennemi
- [ ] (T) Deux ennemis frappent un joueur mourant dans le meme tour : un seul PlayerDied (condition = passage vivant -> mort)
- [ ] (T) Ennemi mort en cours de tour : saute, puis purge de la liste

### 4.4 Mort des ennemis - feature/enemy-views
- [ ] CreatureDied publie par le DamageHandler pour les non-joueurs
      (ApplyDamage retire deja la creature de l'OccupancyMap) ; points de score en Phase 8
- [ ] EntityView reutilisee pour les ennemis : apparition, deplacement, disparition a la mort
- [ ] (T) CreatureDied publie une seule fois, jamais pour le joueur

### 4.5 Donnees des ennemis - feature/enemy-data
- [ ] EnemyData (SO) : nom, PV, degats, VisionRange, randomStepChance, poids de deplacement
      (Stroll et pas aleatoire), score, drop ; remplace MockEnemyConfig
- [ ] 2 types de monstres garantis aux stats distinctes ; 3e type (+)

### 4.6 Projectile - feature/throw-projectile
- [ ] Projectile visuel du lancer de hachette ; (D) input bloque pendant l'animation, ou projectile instantane

### 4.7 Validation
- [ ] (V) Combat jouable a l'ecran, famine fonctionnelle, un monstre peut etre seme

---

## Phase 5 - Generation procedurale

- [ ] (D) Taille de l'etage et des pieces (idee : 21x21, pieces 5x5 max, 1 case de mur autour)
- [ ] 9 pieces en 3x3, couloirs entre voisines, fusion possible (probabilite parametrable)
- [ ] Entree et sortie dans deux pieces differentes
- [ ] Placement des monstres selon une config de densite
- [ ] Seed de run interne (aucune interface) ; seed d'etage derivee de (seed de run, numero d'etage)
- [ ] Tout le hasard passe par une seule instance de System.Random injectee
- [ ] Affichage ASCII de debug (+)
- [ ] (T) 1000 seeds : 100 % des etages completables, sortie != entree
- [ ] (T) Toutes les entites placees par le generateur sont sur une case praticable

---

## Phase 6 - Progression et boss

- [ ] RunManager : seed, etage courant, etat (en cours / victoire / defaite)
- [ ] Sortie : etage suivant genere, stats du joueur conservees
- [ ] Nouvel EventBus a chaque etage (aucun abonne fantome)
- [ ] Nombre d'etages normaux parametrable (3 a 5) + etage de boss
- [ ] Difficulte croissante simple via config (+)
- [ ] Etage de boss : une grande piece, le boss garde la sortie
- [ ] Boss = un gros monstre (stats via EnemyData), sans mecanique speciale
- [ ] Victoire = boss mort ; defaite = PV a 0
- [ ] (V) Une run complete se joue du premier etage a la victoire ou la mort

---

## Phase 7 - Items (fusion de l'ancienne 3.3 et de l'ancienne Phase 6)

- [ ] (D) Inventaire en file (3 emplacements) ou usage direct du consommable au ramassage
- [ ] (D) Un item au sol occupe-t-il une case dans l'OccupancyMap ? (impact sur MoveAction et le ramassage)
- [ ] (D) Affichage des items au sol : EntityView ou affichage dedie
- [ ] Donnees d'item (SO) : nom, icone, actif/passif, occupe une place, consommable
- [ ] Loot au sol : nourriture (petite/moyenne/grosse), hachettes (3/5), potion de vie
- [ ] Ramassage automatique en marchant dessus
- [ ] Potion de vie : soin moyen plafonne aux PV max
- [ ] Placement du loot a la generation (config de densite)
- [ ] Si inventaire : file de 3 emplacements remplie de droite a gauche sans trou ;
      plein = ejection de l'item le plus a droite (depose sur la case du joueur), decalage, ajout a gauche ;
      utilisation = consommation puis decalage vers la droite ; HUD 3 emplacements, clic gauche pour utiliser (GDD 4.4)
- [ ] Drops de monstres (+)
- [ ] (T) Ramassage, ressources mises a jour, potion plafonnee ; ResolveMove vers une entite non-creature
- [ ] (T) Si inventaire : ajout, ejection quand plein (l'item ejecte est renvoye), decalage apres utilisation
- [ ] (V) Loot visible, ramassage et utilisation visibles a l'ecran

---

## Phase 8 - Score et State Machine

- [ ] Score : etage atteint + monstres tues ; ponderation dans une config (SO)
- [ ] HUD : completer le squelette (score, zone reservee monnaie)
- [ ] State Machine : Bootstrap -> Menu -> Run -> FinDeRun -> Menu (pas d'etat Pause dans le proto)
- [ ] Menu principal : Nouvelle run, Quitter
- [ ] Ecran de fin : resultat, cause, score, rejouer / menu
- [ ] Mort du joueur : sortie de l'etat Run, desactivation de la map Gameplay
- [ ] (V) Plusieurs runs enchainees sans relancer le jeu, tout est remis a zero

---

## Phase 9 - Build et playtests -> PROTO

- [ ] Build PC teste (pas seulement l'editeur)
- [ ] Playtests : 20 runs completes, noter les points bloquants
- [ ] Equilibrage via les SO : nourriture, PV, hachettes, VisionRange, densites, stats monstres/boss, nombre d'etages
- [ ] Objectif : la famine met la pression sans etre injuste, le boss est un vrai pic
- [ ] Corriger les bugs bloquants ; definir la configuration minimale
- [ ] (V) Criteres du GDD 10.4 valides, hors leaderboard (reporte au palier 0.5)
- [ ] (+) Repasse de proprete : un fichier par type, langue des commentaires, noms des tests

---

## Palier 0.5 - Polish (si temps restant avant le 15/12, dans cet ordre)

1. Assets, son et retours visuels
   - Choix et integration du pack d'assets : tuiles, joueur, monstres, boss, sortie, loot, icones HUD
   - Taille de tuile et Pixels Per Unit selon le pack ; Pixel Perfect Camera (ratio de la Game view)
   - Animations de deplacement / attaque ; ennemis animes ensemble apres resolution du tour
   - Blocage de l'input pendant les animations
   - Verifier le HUD avec les animations : il lit les valeurs actuelles de la Creature, pas celles du moment de l'evenement
   - Sons minimum : pas, coup porte, coup recu, ramassage, mort, musique ; (+) ambiances
   - Retours visuels (degats, mort, ramassage) et textes contextuels, dont la description d'item au survol
   - Police et cadres (verifier les licences)
   - (V) Le jeu est lisible : joueur, ennemis, loot, sortie identifiables d'un coup d'oeil
2. Brouillard : etat par case (jamais vue / deja vue / visible), zone carree de rayon VisionRange (murs ignores),
   noir si jamais vue, assombri si deja vue, ennemis caches hors vue ; (T) etats corrects ; (V) rendu
3. Leaderboard local JSON, top 10 ; nom par defaut, saisie du nom (+) ;
   (T) tri, limite top 10, fichier absent ou corrompu ne plante pas
4. Menu pause (etat Pause de la State Machine, Action Map UI : Echap) : reprendre, abandonner, quitter
5. Attaque a distance des monstres
6. Mecaniques speciales du boss (patterns)

---

## Paliers apres le proto (GDD 10.3)

- Palier 1 : arme, armure, item nourriture -> PV, quelques runes, ceinture (+)
- Palier 2 : monnaie, marchands
- Palier 3 : lootables de score
- Palier 4 : grandes potions, plus de sorts
- Palier 5 : familiers
- Palier a definir : test de touche (Dex), support manette, refacto des inputs directionnels en actions Button plutot que Value, tester alternative = Resolution simultanee du tour : decisions de toutes les creatures -> TurnResolved -> application des changements -> mise en scene (le joueur qui fuit un monstre adjacent prend un coup en passant)

## Hors scope pour le moment

Monnaie, marchands, equipement, bijoux, runes, grandes potions, item de transformation,
familiers, artefacts, autres biomes, Odin, tactile, manette, mecaniques de biome, test de touche.
Hors scope definitif : interface de seed (saisie, affichage, partage).