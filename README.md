# Projet de Recherche : Labyrinthe VR et Étude sur le Cybermalaise

## Description du Projet

Ce projet a pour objectif de développer une expérience immersive en réalité virtuelle (VR) où l'utilisateur navigue dans un labyrinthe 3D généré aléatoirement. L'étude vise à évaluer l'impact de différentes conditions expérimentales sur les symptômes de cybermalaise au fil du temps, en utilisant le **Simulator Sickness Questionnaire (SSQ)**.  

Au fil des itérations, de nombreuses fonctionnalités ont été ajoutées afin d’enrichir l’expérience et de la rendre plus adaptée à l’expérimentation.  

## Nouvelles Fonctionnalités

- **Buzzers interactifs à la place des formulaires fixes** :  
  Les anciens points d’apparition des formulaires dans le labyrinthe ont été remplacés par des buzzers interactifs. Le joueur peut appuyer sur ces buzzers à l’aide de sa manette VR, ce qui déclenche une téléportation vers une scène distincte où il doit remplir le formulaire **QSS**. Une fois le questionnaire complété, le joueur est automatiquement replacé dans le labyrinthe exactement à l’endroit où il avait activé le buzzer.  

- **Sauvegarde des formulaires en JSON** :  
  Chaque formulaire rempli est sauvegardé dans un fichier `.json`. Sur casque VR, ces fichiers se trouvent dans :  
  `Android > Data > Unity.Template.Vr > Files`.  
  Les fichiers JSON conservent l’ordre des buzzers activés (start, mid, end), le scénario courant, ainsi que la date et l’heure précises de la sauvegarde.  

- **Système de scénarios expérimentaux** :  
  Trois scénarios distincts existent, chacun appliquant un filtre de température visuel différent : **chaud**, **froid** ou **neutre**.  
  À chaque fois que l’utilisateur termine le formulaire de fin, le labyrinthe est réinitialisé, le scénario suivant est sélectionné aléatoirement, et l’expérience recommence avec l’ensemble des questionnaires à remplir.  

- **Gestion dynamique des buzzers (StepBuzzer)** :  
  Chaque buzzer disparaît une fois son questionnaire associé rempli. Cette logique est gérée par l’objet persistant **StepBuzzer**, qui survit entre les différentes scènes Unity.  

- **Déplacement uniquement au joystick** :  
  Afin de renforcer l’effet expérimental sur le cybermalaise, l’option de téléportation a été retirée. Le joueur se déplace désormais uniquement grâce au joystick dans le labyrinthe.  

- **Scripts et Organisation** :  
  Tous les nouveaux scripts ont été regroupés dans différents dossiers des *Assets* :  
  - **Scripts Teleport** et **Script** : gestion des buzzers, téléportation, sauvegarde et réapparition.  
  - **Scripts Changer Camera** : scripts de post-traitement permettant de modifier la caméra (température, luminosité, couleur, etc.). Ces scripts peuvent être réutilisés dans d’autres projets Unity en ajoutant un *GameObject Global Volume*, en activant les *Overrides* souhaités et en cochant le *Render Processing* sur la caméra principale.  
  - **Head Motion** : scripts spécifiques qui appliquent des filtres visuels en fonction des mouvements de la tête et du regard de l’utilisateur.  

- **SeedSaver et génération contrôlée du labyrinthe** :  
  Le **SeedSaver** est un *GameObject* persistant qui conserve la *seed* de génération du labyrinthe, ainsi que la position et l’orientation du joueur au moment d’une téléportation. Cela garantit que, lors du rechargement de la scène du labyrinthe, celui-ci reste identique et cohérent avec la progression du joueur. Si aucune seed n’a encore été définie (valeur par défaut 0), une seed aléatoire est alors générée.  

- **Gestion de l’état des buzzers (BuzzerStateManager)** :  
  L’objet persistant **Go_StepBuzzer** utilise le script *BuzzerStateManager* pour contrôler l’état des buzzers via des cases à cocher. Cela permet de gérer leur apparition/disparition en fonction de l’avancement du joueur.  

- **Scènes de formulaires distinctes** :  
  Les questionnaires QSS sont répartis dans trois scènes dédiées : **Start**, **Mid** et **End**, correspondant aux trois étapes clés de la progression dans le labyrinthe.  

## Fonctionnalités Initiales (héritées du projet d’origine)

- **Génération de Labyrinthe 3D** : Un labyrinthe est généré aléatoirement à chaque session, avec des cellules et des murs configurables.  
- **Rétroactions Vibrotactiles** : Plusieurs modes de vibrations sont proposés pour évaluer leur impact sur le cybermalaise :  
  - Aucun retour  
  - Vibration aléatoire  
  - Vibration rythmique  
  - Vibration dépendante de la vitesse  
- **Questionnaires SSQ** : Initialement placés directement dans le labyrinthe, maintenant remplacés par des scènes dédiées accessibles via les buzzers.  
- **Personnalisation des Murs** : Différents styles de murs sont disponibles (Noir et Blanc, RGB).  
- **Gestion des Sessions** : Suivi des sessions utilisateur, y compris la durée et les réponses aux questionnaires.  

## Structure du Projet

### Scripts Clés (nouveaux et anciens)

- **`MazeGenerator.cs`** : Génère le labyrinthe 3D aléatoire et gère la logique des buzzers avec la seed.  
- **`SeedSaver.cs`** : Conserve la seed du labyrinthe, la position du joueur et la rotation de la caméra.  
- **`BuzzerStateManager.cs`** : Gère l’état d’activation/destruction des buzzers.  
- **`StepBuzzer.cs`** : Gère la disparition des buzzers après remplissage d’un questionnaire.  
- **`QuestionnaireManager.cs`** : Affiche et collecte les réponses du questionnaire QSS.  
- **`VRHaptics.cs`** : Implémente les modes de rétroactions vibrotactiles.  
- **`Scripts Changer Camera`** : Scripts de post-processing pour modifier la caméra.  
- **`Head Motion`** : Scripts modifiant les filtres en fonction des mouvements de tête.  

## Instructions d'Utilisation

1. **Configuration Initiale** :  
   - Assurez-vous que les dépendances Unity nécessaires (XR Toolkit, TMP, etc.) sont installées.  
   - Configurez les contrôleurs VR et les paramètres de la scène.  

2. **Lancement de l'Expérience** :  
   - Lancez la scène principale contenant le labyrinthe.  
   - L'utilisateur peut naviguer dans le labyrinthe à l'aide du joystick VR.  

3. **Interaction avec les Buzzers et Questionnaires** :  
   - Les buzzers apparaissent à des points spécifiques (début, milieu, fin).  
   - Lorsqu’un buzzer est activé, le joueur est téléporté vers la scène de formulaire correspondante.  
   - Les réponses sont sauvegardées dans un fichier JSON pour analyse ultérieure.  

4. **Personnalisation** :  
   - Les utilisateurs peuvent choisir le type de mur avant de commencer.  
   - Les vibrations sont automatiquement configurées en fonction du mode sélectionné.  
   - Les filtres caméra (température, couleurs, luminosité) peuvent être activés via les scripts *Changer Camera*.  

## Objectifs de l'Étude

- **Évaluer le Cybermalaise** : Mesurer les symptômes de cybermalaise à l'aide des réponses au SSQ.  
- **Analyser l'Impact des Vibrations** : Comparer les différents modes de rétroactions vibrotactiles pour identifier leur influence sur le confort de l'utilisateur.  
- **Analyser l'Impact des Scénarios** : Étudier l’effet des filtres visuels (chaud, froid, neutre) sur l’expérience utilisateur et les symptômes de cybermalaise.  
- **Améliorer l'Immersion** : Offrir une expérience VR engageante et personnalisable.  

## Technologies Utilisées

- **Unity** : Moteur de jeu pour le développement de l'expérience VR.  
- **C#** : Langage de programmation pour les scripts.  
- **XR Toolkit** : Gestion des interactions VR.  
- **TMP (TextMeshPro)** : Affichage des textes dans l'environnement VR.  

## Contributions

- **Victor Vieux-Melchior** – Développeur initial  
- **Baptiste BERRETTA** – Développeur stagiaire Mitacs  
- **Andrew HIVER** – Développeur stagiaire

Sous la supervision de **Pascal E. Fortin** dans le cadre d'une étude de recherche sur le cybermalaise en réalité virtuelle pour [l'UQAC](https://www.uqac.ca/)   

## Remarques

- Les données utilisateur sont sauvegardées localement dans un fichier JSON.  
- Le projet est conçu pour être extensible, permettant l'ajout de nouveaux modes de vibrations, de scénarios ou de filtres visuels.  
- Ce README ne recense pas de manière exhaustive l’ensemble des modifications du projet, mais présente les plus importantes pour comprendre son évolution.  
