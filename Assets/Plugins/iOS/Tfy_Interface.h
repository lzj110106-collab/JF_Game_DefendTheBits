//
//  Tfy_Interface.h
//  Unity-iPhone
//
//  Created by iMac on 2017/10/17.
//
//

//重写NSLog,Debug模式下打印日志和当前行数
#if DEBUG
#define NSLog(FORMAT, ...) fprintf(stderr,"\nfunction:%s line:%d\n%s\n", __FUNCTION__, __LINE__, [[NSString stringWithFormat:FORMAT, ##__VA_ARGS__] UTF8String]);
#else
#define NSLog(FORMAT, ...) nil
#endif

// UIColor *blue = UIColorFromRGB(0x4799dd);
// UIColor *red = UIColorFromRGB(0xe3372b);
#define UIColorFromRGB(rgbValue) [UIColor colorWithRed:((float)((rgbValue & 0xFF0000) >> 16))/255.0 green:((float)((rgbValue & 0xFF00) >> 8))/255.0 blue:((float)(rgbValue & 0xFF))/255.0 alpha:1.0]

//DEBUG  模式下打印日志,当前行 并弹出一个警告
#ifdef DEBUG
#   define ULog(fmt, ...)  { UIAlertView *alert = [[UIAlertView alloc] initWithTitle:[NSString stringWithFormat:@"%s\n [Line %d] ", __PRETTY_FUNCTION__, __LINE__] message:[NSString stringWithFormat:fmt, ##__VA_ARGS__]  delegate:nil cancelButtonTitle:@"Ok" otherButtonTitles:nil]; [alert show]; }
#else
#   define ULog(...)
#endif


#define AGENT @"app-12-01"

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface Tfy_Interface : UIViewController<SKProductsRequestDelegate,SKPaymentTransactionObserver>
+(id)Tfy_local;
- (void)InitCallBack;
-(void)PurchaseItem:(float)price purchaseID:(const char *)a purchaseItem:(const char *)b purchaseDiscribe:(const char *)c userID:(const char *)d;
-(void)InitSDKIn;
-(void) paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions;
-(void) completeTransaction: (SKPaymentTransaction *)transaction;
-(void) failedTransaction: (SKPaymentTransaction *)transaction;

@end

