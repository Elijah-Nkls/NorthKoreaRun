##NORTH KOREA RUN

#MITGLIEDER:
Niklas, verschiedene Aufgaben, 33%
Elijah, verschiedene Aufgaben, 33%
Runsheng, verschiedene Aufgaben, 33%

Die meiste Arbeit wurde am eigenen PC gemacht und dort getestet, damit möglichst wenig Fehler entstehen. Während der Entwicklung haben wir uns regelmäßig im Team abgesprochen und die Ergebnisse später zusammengeführt.

#SPIELIDEE:
North Korea Run ist ein 2D-Spiel mit Draufsicht (Top-Down) oder Third-Person-Ansicht. Man spielt einen nordkoreanischen Bürger oder eine Bürgerin, die aus der Demokratischen Volksrepublik Korea (DPRK) fliehen will, um ein neues, freies Leben zu starten. Das Spiel ist wie ein Labyrinth aufgebaut: In drei verschiedenen Leveln muss man jeweils Schlüssel finden, um die Tür zum nächsten Abschnitt zu öffnen und zu entkommen. Währenddessen wird man von Wachen verfolgt, was das Ganze spannender macht.

#WOCHENBERICHTE:
02.06-08.06.: Repository erstellt.
Probleme: Keine.

16.06-22.06.: .gitignore hinzugefügt, erste Spielvorlage im Pacman-Stil gebaut.
Probleme: Es gab noch keine Menüs, keine Level, nur eine Steuerung, wenig eigene Ideen, die Spielfigur war schwer zu steuern, Gegner liefen nicht richtig und die Karten waren nicht fertig.

23.06-29.06.: KI verbessert, neue Karten und Grafiken gemacht, Easter Eggs eingebaut.
Probleme: Bilder einzubauen war teilweise schwierig.

30.06-04.07.: Kleine Fehler behoben, Code besser kommentiert.
Probleme: Viele Probleme beim Mergen in Git, manchmal mussten neue Repositories erstellt werden.

#UMSETZUNG:
Das Spiel wurde in C# mit Windows Forms programmiert. Die Spiellogik läuft über eine Labyrinthstruktur, die als Liste von Strings gespeichert ist. Die Spielfigur und die Gegner bewegen sich im Labyrinth, wobei die Gegner eine einfache KI haben und auf den Spieler zulaufen.

#WICHTIGE FEATURES:
Drei Level, jedes mit eigenem Labyrinth

Schlüssel als wichtiges Spielelement

Gegner mit KI, die den Spieler verfolgen

Easter Eggs wie der Kim-Mode

Cheat-Modus (Super-Speed und Unverwundbarkeit)

Startmenü und Endbildschirm

Gut kommentierter und übersichtlicher Code

#STEUERUNG:
Bewegung: Pfeiltasten

Cheat-Modus: Strg + Alt + L

Kim-Mode aktivieren: "KIM" auf der Tastatur eingeben

Kim-Mode deaktivieren: "RESET" eingeben