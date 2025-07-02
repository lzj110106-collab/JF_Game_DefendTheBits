
#ifndef __IAP_CLASS_H__
#define __IAP_CLASS_H__

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
//#import "CangShuBundleAd.h"

enum{
    IAPp1=1,
}buyCoinsTag;

//内购------------------------------------------
@interface CIAPClass : UIViewController<SKProductsRequestDelegate,SKPaymentTransactionObserver>
{
    NSString* buyType;
    float nCost;
    NSString* itemId;
    NSString* itemDes;
    //bool* isPurchasing;
}
+(id)sharedManager;
-(void) requestProUpgradeProductData;
-(void) RequestProductData;
-(bool) CanMakePay;
-(void) buy:(NSString*)type cost:(float)cost purchaseID:(NSString*)itemID purchaseDiscribe:(NSString*)dis;
-(void) paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions;
-(void) PurchasedTransaction: (SKPaymentTransaction *)transaction;
-(void) completeTransaction: (SKPaymentTransaction *)transaction;
-(void) failedTransaction: (SKPaymentTransaction *)transaction;
-(void) paymentQueueRestoreCompletedTransactionsFinished: (SKPaymentTransaction *)transaction;
-(void) paymentQueue:(SKPaymentQueue *) paymentQueue restoreCompletedTransactionsFailedWithError:(NSError *)error;
-(void) restoreTransaction: (SKPaymentTransaction *)transaction;
-(void) provideContent:(NSString *)product;
-(void) recordTransaction:(NSString *)product;
-(NSString*) getPaymentStateStr:(SKPaymentTransactionState)payment;

-(void) restore;
-(void) ADInit;
-(void) BannerAD;
-(void) InterAD;

-(void)setTransitionReceipt:(NSString *)payload receipt:(NSString *)receipt transactionId:(NSString *)transactionId;
@end


#endif//__IAP_CLASS_H__
