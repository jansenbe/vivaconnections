import { IPropertyPaneConfiguration } from '@microsoft/sp-property-pane';
import { BaseAdaptiveCardExtension } from '@microsoft/sp-adaptive-card-extension-base';
import { CardView } from './cardView/CardView';
import { QuickView } from './quickView/QuickView';
import { DetailedQuickView } from './quickView/DetailedQuickView';
import { CallingHomePropertyPane } from './CallingHomePropertyPane';
import { AadHttpClient, HttpClientResponse } from '@microsoft/sp-http';
import * as strings from 'CallingHomeAdaptiveCardExtensionStrings';
import { Tokens } from './services/Tokens';

export interface ICallingHomeAdaptiveCardExtensionProps {
  title: string;
  description: string;
  iconProperty: string;
  apiAbsUrl: string;
  appId: string;
}

export interface ICallingHomeAdaptiveCardExtensionState {
  description: string;
  apiCallResults: IOrder[];
  currentIndex: number;
  processing: boolean;
  licensed: boolean;
}

export interface IOrder {
  id: number;
  orderDate: string;
  region: string;
  rep: string;
  item: string;
  units: number;
  unitCost: number;
  total: number;
}

const CARD_VIEW_REGISTRY_ID: string = 'CallingHome_CARD_VIEW';
export const QUICK_VIEW_REGISTRY_ID: string = 'CallingHome_QUICK_VIEW';
export const DETAILED_QUICK_VIEW_REGISTRY_ID: string = 'CallingHome_DETAILED_QUICK_VIEW';


export default class CallingHomeAdaptiveCardExtension extends BaseAdaptiveCardExtension<
  ICallingHomeAdaptiveCardExtensionProps,
  ICallingHomeAdaptiveCardExtensionState
> {
  private _deferredPropertyPane: CallingHomePropertyPane | undefined;

  public onInit(): Promise<void> {
    this.state = {
      description: this.properties.description,
      apiCallResults: [],
      currentIndex: 0,
      processing: false,
      licensed: false,
    };

    this.cardNavigator.register(CARD_VIEW_REGISTRY_ID, () => new CardView());
    this.quickViewNavigator.register(QUICK_VIEW_REGISTRY_ID, () => new QuickView());
    this.quickViewNavigator.register(DETAILED_QUICK_VIEW_REGISTRY_ID, () => new DetailedQuickView());

    // check for a valid "ISV product license"
    return this.licenseCheck();

    // Perform an async load of the needed data and store it in the state
    //return this.callApi();
  }

  private async licenseCheck(): Promise<void> {

    if (this.state.processing) {
      // we have a pending request
      return;
    }

    this.setState({processing : true, licensed : false});

    try {
      // Instantiate aad http client for the AAD application we've created
      const client = await this.context.aadHttpClientFactory.getClient(this.properties.appId);
      
      // Build the request to call the API
      const reqUrl = (new URL("/api/licensecheck", this.properties.apiAbsUrl)).toString();
      
      // Make the API call, send information about the current user, site and tenant
      const res: HttpClientResponse = await client.post(reqUrl, AadHttpClient.configurations.v1, {      
        body: JSON.stringify({
          user: this.context.pageContext.user,
          siteId: this.context.pageContext.site.id.toString(),
          siteUrl: this.context.pageContext.site.absoluteUrl,
          webId: this.context.pageContext.web.id.toString(),
          webRelUrl: this.context.pageContext.web.serverRelativeUrl,
          webAbsUrl: this.context.pageContext.web.absoluteUrl,
          aadInfo: {
            tenantId: this.context.pageContext.aadInfo.tenantId.toString(),
            userId: this.context.pageContext.aadInfo.userId.toString(),
          },
        })
      });

      // Process API call response
      if (!res.ok) {

        console.log('XYZ License check API call failed!');

        // Store the json in the card state
        this.setState({
          apiCallResults : [], 
          description : strings.NotLicensed, 
          licensed : false
        });

        const resp = await res.text();
        console.error(Error(`Error [${res.status}: ${res.statusText}]: ${resp}`));

      } else {

        console.log('XYZ License check API called!');

        const jsonResponse = await res.json();        

        // For demo purposes, randomly pick licensed/unlicensed
        var randomBoolean = jsonResponse.IsLicensed;
        //var randomBoolean = Math.random() < 0.5;
        //const randomBoolean : boolean = false;

        if (randomBoolean) {

          // Option: Cache the received access token for future reuse
          const tokens = Tokens.getInstance();
          tokens.setAccessToken(jsonResponse.AccessToken);

          // Call XYZ API to retrieve needed data
          await this.getOrders(jsonResponse.AccessToken);

          // Licensed: Store the json in the card state
          this.setState({
            description : strings.Licensed, 
            licensed : true
          });
        }
        else {
          // Unlicensed: Store the json in the card state
          this.setState({
            description : strings.NotLicensed, 
            licensed : false
          });
        }
      }

    } catch (e) {
      console.error(e);
    } finally {
      this.setState({processing : false});
    }
  }
  
  private async getOrders(accessToken: string): Promise<void> {
    try {
      // Instantiate aad http client for the AAD application we've created
      const client = await this.context.aadHttpClientFactory.getClient(this.properties.appId);
      // Build the request to call the API
      const reqUrl = (new URL("/api/Orders", this.properties.apiAbsUrl)).toString();
      // Make the API call
      const res: HttpClientResponse = await client.post(reqUrl, AadHttpClient.configurations.v1, {  
        body: JSON.stringify({ accessToken : accessToken})    
      });

      // Process API call response
      if (!res.ok) {

        const resp = await res.text();
        console.error(Error(`Error [${res.status}: ${res.statusText}]: ${resp}`));

      } else {

        console.log('XYZ Products API called!');

        const jsonResponse = await res.json();

        const items = jsonResponse.items.map((item: any) => { 
          return { id: item.id, 
                   orderDate : item.orderDate,                   
                   region: item.region,
                   rep: item.rep,
                   item: item.item,
                   units: item.units,
                   unitCost: item.unitCost,
                   total: item.total
                 }; 
        });

        this.setState( { apiCallResults: items });

        console.log(this.state.apiCallResults);
      }

    } catch (e) {

      console.error(e);

    } finally {

    }
  }


  public get title(): string {
    return this.properties.title;
  }

  protected get iconProperty(): string {
    return this.properties.iconProperty || require('./assets/SharePointLogo.svg');
  }

  protected loadPropertyPaneResources(): Promise<void> {
    return import(
      /* webpackChunkName: 'CallingHome-property-pane'*/
      './CallingHomePropertyPane'
    )
      .then(
        (component) => {
          this._deferredPropertyPane = new component.CallingHomePropertyPane();
        }
      );
  }

  protected renderCard(): string | undefined {
    return CARD_VIEW_REGISTRY_ID;
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return this._deferredPropertyPane!.getPropertyPaneConfiguration();
  }

  // tslint:disable-next-line: no-any
  protected onPropertyPaneFieldChanged(propertyPath: string, oldValue: any, newValue: any): void {
    this.setState({
      description: newValue
    });
  }
}
