# Feedback på Lufthavns Informationssystem ✈️

Rigtig godt arbejde! Din løsning er meget komplet, og du har demonstreret en stærk forståelse for både Web API og asynkron kommunikation med RabbitMQ.

Her er mine observationer og forslag:

### 1. ASP.NET Core Web API
*   **Controllers & Endpoints:** Du har alle de krævede endpoints (POST, PUT, DELETE). En lille detalje: Din route var sat til `api/controller`, hvilket gør routen bogstavelig. Jeg har rettet den til `api/[controller]`, så den automatisk bruger klassens navn (`Flights`).
*   **Data Modeller:** Din `Flight` model er god. Overvej at bruge en `enum` til `Status` i fremtiden for at undgå stavefejl (f.eks. "Delated" vs "Delayed").

### 2. RabbitMQ Integration (Bonusopgaven! 🌟)
*   **Topics:** Det er super sejt, at du har implementeret Topic Exchange! Det er den helt rigtige måde at håndtere forskellige terminal-typer på. Din logik med routing keys (`flight.domestic` osv.) er spot on.
*   **Connection Handling (Optimering):** I din oprindelige kode oprettede du en ny forbindelse og kanal for hver eneste besked. Det er meget "dyrt" i RabbitMQ. Jeg har rettet din `RabbitMQProducer`, så den genbruger forbindelsen (Singleton), hvilket gør dit system langt mere effektivt.

### 3. Konsol Applikation (Flight Info Screen)
*   **Brugeroplevelse:** Din interaktive menu til valg af terminal og dit farvekodede board ser fantastisk ud! Det er meget brugervenligt.
*   **Logik:** Din måde at opdatere listen i hukommelsen på (`flightBoard`) fungerer rigtig godt.

### 4. Generel Kodekvalitet
*   **Dependency Injection:** Flot brug af interfaces (`IMessageProducer`).
*   **Værktøjer:** Godt set at bruge `Scalar` til din API-dokumentation – det er et lækkert moderne alternativ til Swagger.

### Forslag til næste skridt
*   Prøv at flytte `Flight` modellen ud i et "Shared" klassebibliotek, så du ikke skal have den samme fil liggende to steder (både i API og Konsol-app).

Se mine rettelser i koden for at se, hvordan Singleton-mønsteret implementeres til RabbitMQ. Fortsæt det gode arbejde!
