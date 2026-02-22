# 🌍 Guide de Test du Changement de Langue

## 📋 Étapes de Débogage

### 1. **Ouvrir la Console du Navigateur (F12)**

```
Ctrl + Shift + I  (Windows/Linux)
Cmd + Option + I  (Mac)
```

### 2. **Copier ce code dans la console**

```javascript
// Tester directement la fonction setLanguage
console.log("🧪 Testing Language Switch...");
window.appLanguage.setLanguage("DE");
```

### 3. **Vérifier les logs dans la console**

Vous devriez voir :

```
🔄 Changement en cours... DE
✅ Langue mise à jour: {status: "success", language: "DE", ...}
```

---

## 🔧 Test de l'Endpoint API

### Via PowerShell (Windows)

```powershell
$body = @{ lang = "DE" } | ConvertTo-Json
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$response = Invoke-WebRequest -Uri 'https://localhost:7083/Cars/set-language' `
  -Method POST `
  -Headers @{ "Content-Type" = "application/json" } `
  -Body $body `
  -UseBasicParsing
Write-Host $response.Content
```

### Via cURL (Terminal)

```bash
curl -X POST https://localhost:7083/Cars/set-language \
  -H "Content-Type: application/json" \
  -d '{"lang":"DE"}' \
  --insecure
```

### Résponse Attendue

```json
{
  "status": "success",
  "language": "DE",
  "message": "Language updated successfully",
  "timestamp": "2026-02-22T12:00:00Z"
}
```

---

## ❌ Dépannage

### Si le bouton ne répond pas :

1. **Ouvrir DevTools (F12) → Onglet Network**
   - Cliquer sur le drapeau dans la navbar
   - Voir la requête POST `/Cars/set-language`
2. **Vérifier l'état HTTP**
   - ✅ 200 OK → Succès
   - ❌ 400 Bad Request → Données invalides
   - ❌ 404 Not Found → Endpoint inaccessible
   - ❌ 500 Server Error → Erreur serveur

3. **Vérifier la Console (F12)**
   - Chercher les messages d'erreur
   - Taper `window.appLanguage.LANGUAGES` pour voir les langues disponibles

---

## 📝 Logs à Vérifier

### Dans la Console du Navigateur

```
🌐 Initializing Language Manager...
✓ Endpoint /Cars/set-language is accessible
🔄 Changement en cours... DE
Response status: 200
✅ Langue mise à jour: {...}
```

### Dans les Logs Serveur (Terminal)

```
🌐 SetLanguage called with request: DE
🌍 Setting language to: DE
✓ Language saved to session: DE
```

---

## ✅ Checklist de Validation

- [ ] **Console du Navigateur** : Pas d'erreurs JavaScript
- [ ] **Network Tab** : La requête POST répond avec 200
- [ ] **Logs Serveur** : Message "✓ Language saved to session"
- [ ] **UI** : Le bouton affiche la nouvelle langue
- [ ] **localStorage** : `userLanguage` est mis à jour

Pour vérifier localStorage dans la console :

```javascript
console.log(localStorage.getItem("userLanguage")); // Devrait afficher "DE"
```
