Projekta uzdevums
Tesla automašīnas īres platforma (back-end risinājums C# un SQLite)
Izveido vienkāršu Tesla automašīnu īres platformas prototipu. Platformas pamatdoma ir nodrošināt, ka klients var iznomāt Tesla automašīnu, reģistrēties sistēmā un beigās veikt maksājumu par īres periodu.

Risinājums jāveido tikai back-end daļai, C# valodā, par pamatu izmantojot datubāzi ar SQLite.

Sistēmas prasības un funkcionalitāte
Tesla automašīnu saraksts
Katrai Tesla automašīnai jābūt šādiem parametriem:
ID (unikāls identifikators)
Modelis (piem., Model 3, Model Y, utt.)
Stundas likme (EUR/h)
Kilometra likme (EUR/km)
Šos datus glabā SQLite datubāzē.

Klientu reģistrācija
Katram klientam jābūt šādiem parametriem:
ID (unikāls identifikators)
Vārds un uzvārds
E-pasta adrese
Klientu dati arī jāglabā SQLite datubāzē.

Tesla auto īres process
Klients var iznomāt kādu no pieejamajiem Tesla automašīnu modeļiem.
Īres brīdī sistēmā jāreģistrē:
Īres sākuma laiks
Īres beigu laiks (kad īre tiks pabeigta)
Nobraukto kilometru skaits (reģistrē pēc īres beigām)
Maksājumu aprēķins

Maksājums tiek veikts pēc īres beigām.
Maksājuma aprēķins = (Īres ilgums stundās) × (Stundas likme) + (Nobrauktie kilometri) × (Kilometra likme).
Maksājuma kopsumma tiek saglabāta datubāzē.

Svarīgi: Saglabājiet kodus GitHub v ai GitLab
