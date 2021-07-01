import { IPropertyPaneConfiguration, PropertyPaneCheckbox, PropertyPaneTextField } from '@microsoft/sp-property-pane';
import * as strings from 'CallingHomeAdaptiveCardExtensionStrings';

export class CallingHomePropertyPane {
  public getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: { description: strings.PropertyPaneDescription },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('title', {
                  label: strings.TitleFieldLabel
                }),
                PropertyPaneTextField('iconProperty', {
                  label: strings.IconPropertyFieldLabel
                }),
                PropertyPaneTextField('description', {
                  label: strings.DescriptionFieldLabel,
                  multiline: true
                }),
                PropertyPaneTextField('apiAbsUrl', {
                  label: 'Absolute service URL'
                }),
                PropertyPaneTextField('appId', {
                  label: 'Azure AD Application ID'
                }),
                PropertyPaneCheckbox('randomLicensed', {
                  text: 'Use random licensed check'
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
