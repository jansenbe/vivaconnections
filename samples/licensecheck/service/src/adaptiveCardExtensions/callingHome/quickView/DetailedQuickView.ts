import { ISPFxAdaptiveCard, BaseAdaptiveCardView, IActionArguments } from '@microsoft/sp-adaptive-card-extension-base';
import * as strings from 'CallingHomeAdaptiveCardExtensionStrings';
import { ICallingHomeAdaptiveCardExtensionProps, ICallingHomeAdaptiveCardExtensionState } from '../CallingHomeAdaptiveCardExtension';
import { IOrder } from '../CallingHomeAdaptiveCardExtension';

export interface IDetailedQuickViewData {
    item: IOrder;
}

export class DetailedQuickView extends BaseAdaptiveCardView<
  ICallingHomeAdaptiveCardExtensionProps,
  ICallingHomeAdaptiveCardExtensionState,
  IDetailedQuickViewData
> {
  
  public get data(): IDetailedQuickViewData {   
    return { item: this.state.apiCallResults[this.state.currentIndex]  };
  }

  public get template(): ISPFxAdaptiveCard {
    return {
      $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
      type: 'AdaptiveCard',
      version: '1.2',
      body: [
        {
          type: 'ColumnSet',
          columns: [
            {
              type: 'Column',
              items: [
                {
                  type: 'TextBlock',
                  text: '${item.rep}',
                  size: 'ExtraLarge'
                },
                {
                  type: 'TextBlock',
                  text: '${item.item}',
                  size: 'Medium'
                }
              ]
            },
            {
              type: 'Column',
              style: 'emphasis',
              items: [
                {
                  type: 'TextBlock',
                  text: '${item.region}',
                  weight: 'Lighter'
                }
              ]
            }
          ]
        },
        {
          type: 'ActionSet',
          actions: [
            {
              type: 'Action.Submit',
              title: 'Back',
              data: {
                id: 'back'
              }
            }
          ]
        }
      ]
    };
  }

  public onAction(action: IActionArguments): void {
    if (action.type === 'Submit') {
      const { id } = action.data;
      if (id === 'back') {
        this.quickViewNavigator.pop();
      }  
    }
  }
  

}