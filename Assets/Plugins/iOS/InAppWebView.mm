#import <UIKit/UiKit.h>
#include "InAppWebView.h"

void LaunchInAppWebView(char* url)
{
	auto nsurl = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
	NSLog(@"LaunchInAppWebView: %@", nsurl);
	
	//generate thew new UIWebView and load up the requested page
	auto webview = [[UIWebView alloc] initWithFrame:[[UIScreen mainScreen] applicationFrame]];
	[webview loadRequest:[NSURLRequest requestWithURL:nsurl]];
	
	//grab the application view. could do this via casting the app
	//delegate, but this also works with modal views etc
	auto window = [[UIApplication sharedApplication] keyWindow];
	auto topView = [[window subviews] lastObject];
	
	//display it
	[topView addSubview:webview];
}
