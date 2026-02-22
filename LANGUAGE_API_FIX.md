# 🔧 Correction HTTP 405 - Language API Endpoint

## ❌ Problème Identifié

**Erreur 405 Method Not Allowed** lors du changement de langue.

Cause : L'endpoint `/Cars/set-language` n'acceptait pas les requêtes POST correctement car le contrôleur MVC n'avait pas le routage API configuré correctement.

---

## ✅ Solutions Apportées

### 1. **Endpoint Corrigé dans le Contrôleur**

```csharp
// AVANT (Incorrect - 405 error)
[HttpPost("set-language")]
public IActionResult SetLanguage([FromBody] LanguageRequest request)

// APRÈS (Correct)
[HttpPost("/api/set-language")]
[Consumes("application/json")]
[Produces("application/json")]
public IActionResult SetLanguage([FromBody] LanguageRequest request)
```

**Changements :**

- ✅ Route absolue `[HttpPost("/api/set-language")]` au lieu de relative
- ✅ Ajout de `[Consumes("application/json")]` pour clarifier le content-type attendu
- ✅ Ajout de `[Produces("application/json")]` pour le content-type retourné
- ✅ Meilleur logging pour le debug

### 2. **JavaScript met à jour l'URL**

```javascript
// AVANT
fetch('/Cars/set-language', { ... })

// APRÈS
fetch('/api/set-language', { ... })
```

---

## 🧪 Tester la Correction

### Via Console du Navigateur (F12)

```javascript
window.appLanguage.setLanguage("DE");
```

Vous devriez voir:

```
🔄 Changement en cours... DE
Response status: 200
✅ Langue mise à jour: {status: "success", ...}
```

### Via PowerShell

```powershell
$body = @{ lang = "DE" } | ConvertTo-Json
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
Invoke-WebRequest -Uri 'https://localhost:7083/api/set-language' `
  -Method POST `
  -Headers @{ "Content-Type" = "application/json" } `
  -Body $body `
  -UseBasicParsing
```

**Résultat Attendu :**

```
{"status":"success","language":"DE","message":"Language updated successfully","timestamp":"2026-02-22T..."}
```

---

## 📝 Endpoints Disponibles

| Endpoint              | Méthode | Description                  |
| --------------------- | ------- | ---------------------------- |
| `/api/set-language`   | POST    | Change la langue (NEW ✅)    |
| `/Cars/api/translate` | GET     | Traduit le texte             |
| `/Cars/set-language`  | POST    | Ancien endpoint (DEPRECATED) |

---

## 🔍 Logs Serveur

Vous devriez voir dans les logs:

```
🌐 SetLanguage API called with request: DE
🌍 Setting language to: DE
✓ Language saved to session: DE
```

---

## ✅ Checklist Finale

- [ ] Compilé sans erreurs (`dotnet build`)
- [ ] Relancé l'application (`dotnet run`)
- [ ] Cliqué sur le drapeau 🌍 dans la navbar
- [ ] Sélectionné une langue (DE, EN, ES, etc.)
- [ ] Vérifiez la console (F12) - pas d'erreur 405
- [ ] Vérifiez Network tab - requête POST retourne 200
- [ ] La langue a changé dans le sélecteur

---

## ⚡ Prochaines Étapes

1. La traduction devrait maintenant fonctionner correctement
2. L'endpoint `/api/set-language` accepte maintenant les requêtes POST
3. Le JavaScript communique avec l'API sans erreur 405
