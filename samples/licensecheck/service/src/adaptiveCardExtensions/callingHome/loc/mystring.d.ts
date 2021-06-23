declare interface ICallingHomeAdaptiveCardExtensionStrings {
  PropertyPaneDescription: string;
  BasicGroupName: string;
  DescriptionFieldLabel: string;
  TitleFieldLabel: string;
  IconPropertyFieldLabel: string;
  Title: string;
  SubTitle: string;
  Description: string;
  PrimaryText: string;
  QuickViewButton: string;

  Licensed: string;
  NotLicensed: sting;
  LearnMoreButton: string;
}

declare module 'CallingHomeAdaptiveCardExtensionStrings' {
  const strings: ICallingHomeAdaptiveCardExtensionStrings;
  export = strings;
}
