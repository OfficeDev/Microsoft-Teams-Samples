# Bot Framework SDK Bug Fix

## Problem

The Bot Framework SDK (v4.17.0) has a bug in `oauth_prompt.py` where it attempts to access `.token` attribute on an `ErrorResponse` object, causing an `AttributeError` during failed token exchanges.

**Error**: `AttributeError: 'ErrorResponse' object has no attribute 'token'`

## Solution

This project uses a **custom OAuthPrompt wrapper** that fixes the bug while keeping all other SDK functionality.

### File Structure

```
custom_oauth_prompt.py          ← Single file with bug fix (130 lines)
dialogs/main_dialog.py          ← Uses custom wrapper
```

### Why This Approach?

 **Survives package reinstalls** - No need to patch SDK files  
 **Minimal custom code** - Only 1 file (~130 lines)  
 **Easy to maintain** - Clear documentation  
 **Easy to remove** - When SDK is fixed, just change the import  

### How It Works

The custom `OAuthPrompt` class:
1. Inherits from the SDK's `OAuthPrompt`
2. Overrides only `_recognize_token()` method
3. Adds `hasattr(token_exchange_response, 'token')` check
4. Falls back to parent implementation for all other cases

### Usage

```python
# Instead of:
from botbuilder.dialogs.prompts import OAuthPrompt

# Use:
from custom_oauth_prompt import OAuthPrompt
```

Everything else works exactly the same!

### When to Remove

Once Microsoft fixes this bug in the Bot Framework SDK:
1. Check release notes for the fix
2. Upgrade: `pip install --upgrade botbuilder-dialogs`
3. Change import back to: `from botbuilder.dialogs.prompts import OAuthPrompt`
4. Delete `custom_oauth_prompt.py`
5. Delete this documentation file

---

**SDK Version**: `botbuilder-dialogs==4.17.0`  
**Bug Fixed**: October 2025  
**Custom File**: `custom_oauth_prompt.py`
