- Actions abstrahieren und Handler implementieren
	- (First/Second)Initial(Settlement/Road)Action
- Besseres Interface f�r Bank/Port trades implementieren (Dropdown f�r Zielressource)
- Tracken, ob Dev Cards bereits eine Runde alt sind und gespielt werden k�nnen
- Edit Mode im Interface klar abtrennen
- Longest Road und Largest Army tracken
- Visible/Hidden Victory Points berechnen
- Nach jeder VP-�nderung checken, ob das Match entschieden ist
- Agents abstrahieren (z.B. RandomAgent, MCTSAgent)
- Sounds auch f�r Actions spielen
- W�rfel-Widget synchronisieren
- "Sync Player" Checkbox, um immer das richtige Card Widget zu sehen
- Ranking System

Quellen lesen:
- Catan MCTS => https://www.researchgate.net/publication/220716999_Monte-Carlo_Tree_Search_in_Settlers_of_Catan
- MCTS Variant Review => https://www.researchgate.net/publication/362115589_Monte_Carlo_Tree_Search_a_review_of_recent_modifications_and_applications
- MCTS Tree State Hashes => https://github.com/uranium62/xxHash
- Multiplayer Rating => https://light-and-code.com/?p=122, https://github.com/FigBug/Multiplayer-ELO