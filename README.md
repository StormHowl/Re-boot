# RE-boot

Re-boot est un projet de réalisation d'un jeu vidéo de type FPS Multijoueur avec une mécanique particulière basée sur le management du temps au sein d'un deathmatch entre deux équipes de joueurs, dans une arène.

## Présentation du projet

Le jeu est un FPS classique sous forme de combat d'arène. Chaque joueur lorsqu'il entre dans le jeu a la possibilité de choisir un type de personnage (habituels tank/dps/"heal"), suivant les choix qu'auront fait son équipe de [2-3] personnes. 

Une fois dans le jeu, les joueurs se retrouvent regroupés par équipe sur un zone "de respawn" leur offrant invincibilité et immunité à la capacité Rewind de retour en arrière. En effet, chaque joueur dispose de la capacité "Rewind" qui permet lorsqu'activée de remonter la totalité du jeu en arrière de `x` secondes. Les joueurs morts reviennent à la vie, les points de vie sont restaurés, le décor est restauré. Afin d'éviter les abus, chaque utilisation implique un temps de recharge relatif à chaque niveau : un temps de recharge pour tous les joueurs, un pour tous les joueurs d'une équipe et enfin un pour le joueur de l'équipe en particulier.

Chaque joueur dispose d'un personnage spécialisé dans un certain domaine et disposant [pour le moment] d'une arme qui lui est propre. Chaque arme tire des balles qui lui sont spécifiques, avec un degré de dispersion différent et une distance et cadence de tir différentes (différence entre le sniper et le fusil à pompe).

Le jeu est une carte disposant de reliefs sur le terrain et d'éléments de décors. Une notion de craft permet aux joueurs de récupérer des matériaux lorsqu'un élément du décor est détruit lorsqu'ils utilisent l'outil spécialisé pour la récupération. Ils peuvent avec ces matériaux construire des éléments simples (murs ...). Tous les éléments de décors (construits ou non) sont destructibles. Si détruits avec une arme, ils ne laissent pas de matériaux, si détruits avec l'arme spécialisée, ils laissent des matériaux. Dans le cas des éléments de décor construits par des joueurs, la quantité de matériaux laissés est inférieure à la quantité nécessaire pour les construire.

Chaque équipe dispose d'un compteur de morts. Lorsqu'un coéquipier meurt, ce compteur diminue. La mort est cependant restaurée (et donc le compteur réaugmente) lorsque la capacité Rewind remonte une mort. L'équipe qui est déclarée gagnante est celle qui parvient à réduire à 0 le compteur de morts de l'équipe adverse.

## Liens utiles

Voir la [Roadmap](https://github.com/StormHowl/Re-boot/blob/master/ROADMAP.md)

Trello du projet : [Trello Re-boot](https://trello.com/b/rRrLLPiK/project)



Projet réalisé par : Paul BRETON, Matthieu FELGINES et Nicolas VIDAL. Spécialité TMJI à l'ENSEIRB-MATMECA.
