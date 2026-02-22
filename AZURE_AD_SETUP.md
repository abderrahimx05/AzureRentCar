# Azure AD Configuration Guide

## Setup Instructions for Azure Active Directory Integration

### 1. Azure AD Application Registration

Follow these steps to register your application in Azure Portal:

1. Go to **Azure Portal** (https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the details:
   - **Name**: AzureCarRental
   - **Supported account types**: Choose based on your needs
   - **Redirect URI**:
     - Platform: Web
     - URI: `https://localhost:7177/signin-oidc`

### 2. Update Application Settings

After registration, update your `appsettings.json`:

```json
{
  "Azure": {
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "YOUR_TENANT_ID",
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET",
      "CallbackPath": "/signin-oidc",
      "Domain": "YOUR_TENANT.onmicrosoft.com"
    }
  }
}
```

Where:

- **TenantId**: Found in Azure Portal > Azure Active Directory > Properties > Tenant ID
- **ClientId**: Application (client) ID
- **ClientSecret**: Client secret (create one in Certificates & secrets)

### 3. Create Client Secret

1. In App registration > Certificates & secrets
2. Click **New client secret**
3. Set expiration and description
4. Copy the **Value** (not ID) to `appsettings.json`

### 4. Grant API Permissions

1. In App registration > API permissions
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Delegated permissions**
5. Add:
   - `User.Read`
   - `email`
   - `profile`

### 5. Update Redirect URI

For production, add:

- `https://yourdomain.com/signin-oidc`

### 6. Environment Variables (Security)

For production, use environment variables instead of `appsettings.json`:

```bash
set AZURE_TENANT_ID=your_tenant_id
set AZURE_CLIENT_ID=your_client_id
set AZURE_CLIENT_SECRET=your_client_secret
```

Then update `Program.cs` to read from environment variables:

```csharp
var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
```

## Features Implemented

✅ **Microsoft Azure AD Authentication**

- OAuth 2.0 / OpenID Connect integration
- Automatic user creation on first login
- Role management integration

✅ **Session Management**

- User information stored in session
- Auth provider tracking (Azure vs Local)

✅ **User Synchronization**

- Automatic user creation from Azure AD claims
- Email and name mapping from Azure claims

## Testing

1. Go to `/Auth/Login`
2. Click **"Sign in with Microsoft Azure"**
3. Login with your Azure AD account
4. User is automatically created if first-time login
5. Redirected to Dashboard or Admin panel based on role

## Troubleshooting

**Issue**: "Invalid client id or secret"

- Verify ClientId and ClientSecret in appsettings.json
- Ensure secret hasn't expired

**Issue**: Redirect URI mismatch

- Ensure redirect URI in Azure Portal matches exactly: `https://localhost:7177/signin-oidc`
- For production, update to your domain

**Issue**: "User not found in claims"

- Verify API permissions include `email` and `profile`
- Check Azure AD returns email in claims

## References

- Microsoft Identity Web Docs: https://github.com/AzureAD/microsoft-identity-web
- Azure AD B2C: https://docs.microsoft.com/azure/active-directory-b2c/
- OpenID Connect: https://openid.net/connect/
