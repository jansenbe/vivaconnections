import {
  BasePrimaryTextCardView,
  IPrimaryTextCardParameters,
  IExternalLinkCardAction,
  IQuickViewCardAction,
  ICardButton
} from '@microsoft/sp-adaptive-card-extension-base';
import * as strings from 'CallingHomeAdaptiveCardExtensionStrings';
import { ICallingHomeAdaptiveCardExtensionProps, ICallingHomeAdaptiveCardExtensionState, QUICK_VIEW_REGISTRY_ID } from '../CallingHomeAdaptiveCardExtension';

export class CardView extends BasePrimaryTextCardView<ICallingHomeAdaptiveCardExtensionProps, ICallingHomeAdaptiveCardExtensionState> {
  
  public get cardButtons(): [ICardButton] | [ICardButton, ICardButton] | undefined {

    const buttons: ICardButton[] = [];

    // Only show the quickview button based upon the license check status
    if (this.state.licensed){
      
      if (this.state.authenticated) {
        // User is licensed and authenticated
        buttons.push(
          {
            title: strings.QuickViewButton,
            action: {
              type: 'QuickView',            
              parameters: {
                view: QUICK_VIEW_REGISTRY_ID
              }
            }
          });
      }
      else {
        // User is licensed but still needs to authenticate
        buttons.push(
          {
            title: 'Authenticate',
            action: {
              type: 'ExternalLink',            
              parameters: {
                isTeamsDeepLink: true,
                target: 'https://teams.microsoft.com/l/entity/9cd2aadc-f6be-4029-89ca-4b8a999ff9bd/0'
              }
            }
          });
      }
    }
    else {

      // User is not licensed
      buttons.push(
        {
          title: strings.LearnMoreButton,
          action: {
            type: 'ExternalLink',
            parameters: {
              target: 'https://www.bing.com'
            }
          }
        });
    }

    return buttons as [ICardButton] | [ICardButton, ICardButton];
  }

  public get data(): IPrimaryTextCardParameters {
    return {
      primaryText: strings.PrimaryText,
      description: this.state.description
    };
  }

  public get onCardSelection(): IQuickViewCardAction | IExternalLinkCardAction | undefined {

    if (this.state.licensed){
      return {
        type: 'QuickView',
        parameters: {
          view: QUICK_VIEW_REGISTRY_ID
        }
      };  
    }
    else {
      return {
        type: 'ExternalLink',
        parameters: {
          target: 'https://www.bing.com'
        }
      };  
    }
  }
}
