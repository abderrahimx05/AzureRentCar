# Dépannage du Service de Traduction

## ✅ Corrections Apportées

### 1. **Endpoint `/Cars/set-language` - CORRIGÉ**

**Problème :** Le JavaScript envoyait un JSON POST, mais le contrôleur attendait un paramètre de requête.

**Solution :**

```csharp
// AVANT (Incorrect)
public IActionResult SetLanguage(string lang)  // Attendait un query parameter

// APRÈS (Correct)
public async Task<IActionResult> SetLanguage([FromBody] LanguageRequest request)  // Accepte JSON POST
public class LanguageRequest
{
    public string Lang { get; set; } = "";
}
```

### 2. **Logger Ajouté au CarsController**

Ajout du logger pour tracer les changements de langue :

```csharp
private readonly ILogger<CarsController> _logger;
_logger.LogInformation($"🌍 Language changed to: {lang}");
```

### 3. **Amélioration du JavaScript**

- Meilleure gestion des erreurs
- Messages de notification (si disponible)
- Logs de débogage améliorés

---

## 🧪 Test du Service

### 1. **Tester via Console du Navigateur**

```javascript
// Ouvrir la console (F12)
window.appLanguage.setLanguage("EN");
window.appLanguage.getCurrentLanguage();
```

### 2. **Tester l'API directement**

```bash
# Endpoint de changement de langue
curl -X POST https://localhost:7083/Cars/set-language \
  -H "Content-Type: application/json" \
  -d '{"lang":"EN"}' \
  --insecure

# Endpoint de traduction
curl -X GET 'https://localhost:7083/Cars/api/translate?text=Bonjour&lang=EN' \
  --insecure
```

### 3. **Vérifier l'Endpoint**

```
POST /Cars/set-language
Headers: Content-Type: application/json
Body: {"lang":"EN"}
Response: {"status":"success","language":"EN","message":"Language updated successfully"}
```

---

## 📝 Endpoints API

### ✅ Change Language (Corrigé)

**Route :** `POST /Cars/set-language`  
**Body :** `{"lang":"EN"}` ou `{"lang":"FR"}`  
**Response :**

```json
{
  "status": "success",
  "language": "EN",
  "message": "Language updated successfully"
}
```

### ✅ Translate Text

**Route :** `GET /Cars/api/translate?text=...&lang=...`  
**Response :**

```json
{
  "original": "Bonjour",
  "translated": "Hello",
  "language": "EN"
}
```

---

## 🔍 Points de Vérification

- [ ] Le bouton de langue dans la navbar fonctionne
- [ ] La langue est persistée en localStorage (`userLanguage`)
- [ ] La session côté serveur reçoit le changement de langue
- [ ] L'endpoint `/Cars/api/translate` répond correctement
- [ ] Les logs montrent `🌍 Language changed to: ...`

---

## 🐛 Débogage

Si ça ne fonctionne toujours pas :

1. **Ouvrir les DevTools (F12)**
   - Onglet Network → voir si les requêtes `/Cars/set-language` réussissent
   - Onglet Console → chercher les messages d'erreur

2. **Vérifier la Session**

   ```csharp
   var lang = HttpContext.Session.GetString("UserLanguage");
   // Devrait afficher "EN", "FR", etc.
   ```

3. **Vérifier les Clés Azure**
   - Assurez-vous que `Azure:Translator:ApiKey` est valide dans `appsettings.json`
   - Vérifier dans les logs : "Translator service initialized"

---

## 📌 À Faire

- [ ] Tester le changement de langue dans le navigateur
- [ ] Vérifier la console pour les erreurs
- [ ] Relancer `dotnet run` si les modifications ne s'appliquent pas
