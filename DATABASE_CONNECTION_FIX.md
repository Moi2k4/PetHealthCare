# 🔧 Fixing Supabase Database Connection Timeout

## ⚠️ Current Issue
Your Supabase pooler connection (Port 6543) is timing out when querying data. This is a common issue with Supabase connection pooling.

## 🎯 Solution Options

### Option 1: Use Direct Connection (Recommended)

Supabase provides two connection modes:
- **Pooler** (Port 6543) - Transaction Mode - For serverless functions
- **Direct** (Port 5432) - Session Mode - For persistent connections like your API

#### Steps:
1. Go to your Supabase Project Dashboard
2. Navigate to: **Settings** → **Database**
3. Look for **Connection String** section
4. Find the **Direct connection** string (Port 5432)
5. Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.gcvlsxxegrzoglbcaocl;Password=iM8kTY9p7GhUkuHj;SSL Mode=Require;Trust Server Certificate=true;Timeout=60;Command Timeout=60"
  }
}
```

**Note:** Change Port from `6543` to `5432`

---

### Option 2: Increase Timeout for Pooler Connection

If you must use the pooler, increase timeouts:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.gcvlsxxegrzoglbcaocl;Password=iM8kTY9p7GhUkuHj;SSL Mode=Require;Trust Server Certificate=true;Timeout=120;Command Timeout=120;Keepalive=60;TCP Keepalive=true;TCP Keepalive Time=60;TCP Keepalive Interval=10"
  }
}
```

---

### Option 3: Check Supabase Project Status

Sometimes the issue is with Supabase itself:

1. **Check Project Status**: Go to Supabase Dashboard → Check if project is paused or inactive
2. **Check Network**: Ensure your firewall isn't blocking Supabase
3. **Check Supabase Region**: Make sure you're using the correct region endpoint

---

### Option 4: Use Local PostgreSQL for Development

For faster local development, consider using local PostgreSQL:

#### Install PostgreSQL Locally:
```powershell
# Using Chocolatey
choco install postgresql

# Or download from: https://www.postgresql.org/download/windows/
```

#### Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=petcare;Username=postgres;Password=yourpassword"
  }
}
```

#### Create Database:
```powershell
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE petcare;
\q
```

#### Run Migrations:
```powershell
dotnet ef database update --project PetCare.Infrastructure --startup-project PetCare.API
```

---

## 🧪 Testing the Connection

### Test 1: Check if tables exist
```powershell
# From Supabase Dashboard → SQL Editor, run:
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';
```

### Test 2: Test connection from .NET
Run this command in PowerShell:

```powershell
cd F:\PetCare\PetCare.API
dotnet run
```

Then test an endpoint:
```powershell
Invoke-WebRequest -Uri "https://localhost:54813/api/users?page=1&pageSize=10" -SkipCertificateCheck
```

---

## 📝 Current Connection Settings

I've already updated your `appsettings.json` with improved timeout settings:

```json
"DefaultConnection": "Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.gcvlsxxegrzoglbcaocl;Password=iM8kTY9p7GhUkuHj;SSL Mode=Require;Trust Server Certificate=true;Timeout=60;Command Timeout=60;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10"
```

**But you should still switch to Port 5432 (Direct Connection) for best results.**

---

## 🔍 Get Your Correct Connection String

1. Go to: https://supabase.com/dashboard/project/_/settings/database
2. Look for **Connection string** section
3. Click **URI** tab
4. Select **Transaction mode** or **Session mode**
5. Copy the connection string

**Session mode (Port 5432)** is better for your API application.

---

## ⚡ Quick Fix Commands

After updating the connection string, restart the API:

```powershell
# Stop the API (press Ctrl+C in the terminal running dotnet)

# Then restart:
cd F:\PetCare\PetCare.API
dotnet run
```

---

## 🆘 Still Having Issues?

If none of these work, the issue might be:

1. **Supabase Project Paused**: Check dashboard - free tier projects pause after inactivity
2. **Network/Firewall**: Your network might be blocking Supabase
3. **Invalid Credentials**: Password might have changed
4. **Region Issue**: Verify the correct host endpoint

### Alternative: Skip Database for Now

You can test the API structure without database by creating mock data in the services. Let me know if you want me to create a mock data version for testing!

---

**Recommended Next Step**: Get the Direct Connection string (Port 5432) from Supabase Dashboard and update `appsettings.json`.
