//
//  InAppBrowserViewController.h
//  Unity-iPhone
//
//  Created by Piotr on 04/03/16.
//
//

#import <UIKit/UIKit.h>

@interface InAppBrowserConfig: NSObject {

}

@property (strong) NSString *pageTitle;
@property (strong) UIColor *textColor;
@property (strong) UIColor *barBackgroundColor;
@property (strong) NSString *backButtonText;
@property (nonatomic) BOOL displayURLAsPageTitle;
@property (nonatomic) BOOL hidesTopBar;
@property (nonatomic) BOOL pinchAndZoomEnabled;

+ (InAppBrowserConfig *)defaultDisplayOptions;

@end

@interface InAppBrowserViewController: UIViewController<UIWebViewDelegate> {
    
}

@property (nonatomic, strong) NSString *URL;
@property (nonatomic, strong) NSString *HTML;
@property (nonatomic, strong) InAppBrowserConfig *config;
@property (nonatomic, weak) UIWebView *webView;
@property (nonatomic, weak) UIActivityIndicatorView *indicatorView;

- (void)sendJSMessage: (NSString *)message;
@end
