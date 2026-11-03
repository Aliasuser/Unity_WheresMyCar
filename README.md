# Car_Game
**Unity 2D projekts.**

**Spēles mērķis ir atrast un novietot katru automašīnas attēlu tam paredzētajā vietā, izmērā un rotācijā.**
**Spēles laikā tiek uzņemts laiks, lai sasniegtu labāku rezultātu attēlu ievietošana ir jāveic pēč iespējas ātrāk.**

Spēles kontroles:

Z - Pagriest uz leju

X - Pagriest uz augšu

Space - Apgriezt spoguļskatu

Kreisā bultiņa - Samazināt horizontāli

Labā bultiņa - Palielināt horizontāli

Augšu Bultiņa - Palielināt vertikāli

Uz leju bultiņa - Samazināt vertikāli

## **Izmantotās tehnoloģijas**
* Unity 2D - Spēles dzinējs
* Visual Studio - IDE, kas izmantots priekš C# koda rakstīšanas
* Git - Izmantots priekš versijas kontroles
* Unity Ads - Reklāmas (pilnekrāna, apbalvojuma un baneris)

## **Darāmo darbu saraksts:**
- [x] Izkārtot spēles laukumu (karte) un izvietot automašīnas
- [x] Izstrādāt DragAndDrop skriptu
- [x] Ieviest skaņas efektus un mūziku
- [x] Izstrādāt Transformācijas skriptu (izmērs, rotācija)
- [x] Ievietotas vairāki mašīnas attēli, audio un pievienoti skripti
- [x] Izveidots skripts, kas nejauši mašīnās un vietas izkāŗto pa karti
- [x] Izstrādāt sākuma ainu
- [x] Fonu mūzika
- [x] Pievienots otrais līmenis - Hanojas tornis
- [x] Reklāmu integrācija (pilnekrāna reklāma, baneris, "Watch Ad")
- [x] Pogu skaņas, objektu skaņas
#
> **Spēles sākuma ekrāns**
<img width="564" height="272" alt="image" src="https://github.com/user-attachments/assets/9643573c-0c52-4a03-91b5-d69137ac7eb1" />

> **Spēles pirmais līmenis**
<img width="564" height="272" alt="image" src="https://github.com/user-attachments/assets/6bc5acf6-c5a3-433a-9761-044dec62604d" />

---

# Otrais līmenis - Hanojas tornis (HanoiScene)

**Klasiskā Hanojas torņa spēle ar laika ierobežojumu un zvaigžņu vērtējumu.**

**Spēles mērķis ir pārvietot visus klučus no kreisā staba uz labējo (pēdējo) stabu, saglabājot to secību - lielākais apakšā, mazākais augšā.**
**Jo ātrāk to izdari, jo vairāk zvaigžņu (maksimums 3) saņem.**

> **Hanojas torņa spēles ekrāns**

<img width="627" height="294" alt="image" src="https://github.com/user-attachments/assets/6bba0984-69b4-4085-95e8-100d614751de" />

## **Noteikumi**
* Vienlaikus drīkst pārvietot tikai **augšējo** kluci no staba.
* **Lielāku kluci nedrīkst likt uz mazāka.** Ja mēģini, klucis iemirgojas sarkans un atgriežas iepriekšējā vietā (solis netiek skaitīts).
* Spēle ir uzvarēta, kad visi kluči atrodas uz **labējā staba**.
* Spēles laikā tiek skaitīts **laiks** un **soļu skaits**.

## **Spēles kontroles**
Klučus var pārvietot divos veidos:

* **Vilkšana (Drag & Drop)** - satver augšējo kluci, velc to pie staba un atlaid. Klucis "noslīd" uz stabu.
* **Klikšķis / pieskāriens** - nospied uz augšējā kluča (vai uz staba), un klucis tiek **pacelts augšā**. Pēc tam nospied uz staba, kur to novietot.
  * Nospiežot uz pacelto kluci vai uz tā paša staba, klucis tiek nolaists atpakaļ.
  * Ja izvēlies stabu, kur kluci nedrīkst likt, tas iemirgojas sarkans un paliek pacelts.

Papildu pogas:
* **Menu** - atgriešanās galvenajā izvēlnē
* **Restart** - sākt līmeni no jauna
* **Watch Ad** - noskaties reklāmu, lai **samazinātu laiku** (noklusējumā -40 s) un iegūtu labāku zvaigžņu vērtējumu

## **Zvaigžņu vērtējums**
| Zvaigznes | Laiks (noklusējums) |
|-----------|---------------------|
| ⭐⭐⭐ | līdz 150 s |
| ⭐⭐ | līdz 240 s |
| ⭐ | lēnāk par 240 s |

## **"Watch Ad" - laika samazināšana**
* Noskatoties reklāmu līdz galam, taimerim tiek atņemtas sekundes (nevar nokrist zem 0).
* Vienā mēģinājumā var noskatīties ne vairāk kā **3 reklāmas**.
* Zem pogas redzams skaitītājs, piemēram, `Ads: 0/3` (noskatītās / atļautās). Restart to atiestata.
* Ja reklāma tiek izlaista, atlīdzība netiek piešķirta.

## **Ekrāna orientācija**
* **Hanojas tornis** - tikai **landscape** (horizontāli).
* **Galvenā izvēlne un automašīnu spēle** - noklusējumā portrait, un pagriežot ierīci pāriet uz landscape.

## **Hanojas torņa darāmo darbu saraksts:**
- [x] Izveidot Hanojas torņa ainu un fona attēlu ar stabiem
- [x] Kluču vilkšana
- [x] Kluču pacelšana ar klikšķi un novietošana uz staba
- [x] Laika un soļu skaitīšana
- [x] Zvaigžņu vērtējums pēc laika
- [x] "Watch Ad" laika samazināšana ar skaitītāju
- [x] Pilnekrāna reklāmas un baneris
- [x] Landscape režīms
- [x] Skaņas efekts nederīgai gaitai
