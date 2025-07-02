// This file defines method that are used for writing to the shared defaults that can be read by the Today Widget
// The methods are called by C# methods.

#ifndef Unity_iPhone_SharedPlayerPrefs_h
#define Unity_iPhone_SharedPlayerPrefs_h

#ifndef UDK_NAME
#define UDK_NAME @"group.com.mindcandy.warriors"
#endif

#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

NSUserDefaults* sharedUserDefaults = nil;

extern "C" {
    NSUserDefaults* _sharedPlayerPrefs_getUserDefaults()
    {
        if(sharedUserDefaults == nil)
        {
            sharedUserDefaults = [[NSUserDefaults alloc]initWithSuiteName:UDK_NAME];
            NSLog(@"Shared user defaults\n%@", [sharedUserDefaults dictionaryRepresentation]);
        }
        return sharedUserDefaults;
    }

    void _sharedPlayerPrefs_setFloat(char* key, float value)
    {
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        [shared setFloat:value forKey:GetStringParam(key)];
    }

    void _sharedPlayerPrefs_setInt(char* key, int value)
    {
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        [shared setInteger:value forKey:GetStringParam(key)];
    }

    void _sharedPlayerPrefs_setString(char* key, char* value)
    {
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        [shared setObject:GetStringParam(value) forKey:GetStringParam(key)];
    }

    void _sharedPlayerPrefs_deleteKey(char* key)
    {
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        [shared removeObjectForKey:GetStringParam(key)];
        
    }

    void _sharedPlayerPrefs_deleteAll()
    {
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        NSDictionary * dict = [shared dictionaryRepresentation];
        for (id key in dict) {
            [shared removeObjectForKey:key];
        }
        [shared synchronize];
    }

    void _sharedPlayerPrefs_copyFromDefaultSuite()
    {
        NSUserDefaults *standard = [NSUserDefaults standardUserDefaults];
        NSUserDefaults *shared = _sharedPlayerPrefs_getUserDefaults();
        NSDictionary * dict = [standard dictionaryRepresentation];
        for (id key in dict) {
            id value = [dict valueForKey:key];
            [shared setValue: value forKey: key];
        }
        [shared synchronize];
    }
}

#endif
