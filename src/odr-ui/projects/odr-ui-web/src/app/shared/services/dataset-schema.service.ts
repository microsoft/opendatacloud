import { Injectable } from '@angular/core';
import { Dataset } from '../types';
import { AppConfigurationService } from '../../app-configuration.service';

const datasetSchemaId = "current-dataset-schema";

function valueOrUndefined(value: any) {
  return value === undefined || value === null ? undefined : value;
}

/**
 * Updating head with dataset schema information:
 * https://developers.google.com/search/docs/data-types/dataset
 */
@Injectable()
export class DatasetSchemaService {
    constructor(
      private appConfig: AppConfigurationService
    ) {}

    setSchemaMetaData(dataset: Dataset) {
      const value = {
        "@context": "https://schema.org/",
        "@type": "Dataset",
        name: dataset.name,
        description: dataset.description,
        url: new URL(window.location.href).toString(),
        license: new URL(dataset.licenseContentUri, this.appConfig.current.apiBaseUrl).toString(),
        datePublished: valueOrUndefined(dataset.published),
        version: valueOrUndefined(dataset.version),
        identifier: valueOrUndefined(dataset.digitalObjectIdentifier),
        mainEntityOfPage: valueOrUndefined(dataset.projectUrl),
        keywords: [dataset.domainId],
        creator: {
          "@type": "Organization",
          url: "https://www.microsoft.com/en-us/research/",
          name: "Microsoft Research"
        },
        isAccessibleForFree: true,
        includedInDataCatalog: {
          "@type": "DataCatalog",
          name: "Microsoft Research Open Data",
          url: "https://msropendata.com/"
        }
      };
      const json = JSON.stringify(value, null, 2);
      // console.log("--- metadata ---");
      // console.log(json);

      let current = this.getCurrentDataElement();
      if(!current) {
        current = document.createElement("script");
        current.id = datasetSchemaId;
        current.type = "application/ld+json";
        document.querySelector("head").appendChild(current);
      }

      current.text = json;
    }

    removeSchemaMetaData() {
      const current = this.getCurrentDataElement();
      if(current) {
        current.remove();
      }
    }

    private getCurrentDataElement(): HTMLScriptElement | undefined {
      return document.querySelector(`#${datasetSchemaId}`);
    }
}
