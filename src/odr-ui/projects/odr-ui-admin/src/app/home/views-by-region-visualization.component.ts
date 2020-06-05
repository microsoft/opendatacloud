// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import * as c3 from 'c3';

@Component({
  selector: 'app-views-by-region-visualization',
  template: '<div #renderDiv></div>'
})
export class ViewsByRegionVisualizationComponent implements OnChanges {
  @Input() data: any[];
  @ViewChild('renderDiv', { static: false }) renderDiv: ElementRef;

  ngOnChanges(changes: SimpleChanges): void {
    const { data } = changes;
    if (data && Array.isArray(data.currentValue)) {
      this.renderVisualization(data.currentValue);
    }
  }

  renderVisualization(rawData: any[]) {
    const columns: any = [
      [
        'Names',
        ...rawData.map(({ name }) => name)
      ],
      [
        'Regions',
        ...rawData.map(({ count }) => count)
      ]
    ];

    c3.generate({
      bindto: this.renderDiv.nativeElement,
      size: {
        height: 500,
      },
      bar: {
        width: {
          ratio: 0.75
        }
      },
      padding: {
        top: 50,
        bottom: 50,
        left: 50,
        right: 50
      },
      data: {
        x: 'Names',
        columns,
        groups: [
          ['Regions']
        ],
        type: 'bar',
        labels: {
          format: (_, __, i) => {
            return (rawData[i] && rawData[i].name) || '';
          }
        },
      },
      axis: {
        rotated: true,
        x: {
          type: 'category',
          show: false,
        }
      },
      legend: {
        show: false
      }
    });
  }
}
