// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import * as c3 from 'c3';

@Component({
  selector: 'app-views-by-date-visualization',
  template: '<div #renderDiv></div>'
})
export class ViewsByDateVisualizationComponent implements OnChanges {
  @Input() data: any[];
  @ViewChild('renderDiv', { static: false }) renderDiv: ElementRef;

  ngOnChanges(changes: SimpleChanges): void {
    const { data } = changes;
    if (data && Array.isArray(data.currentValue)) {
      this.renderVisualization(data.currentValue);
    }
  }

  renderVisualization(data: any[]) {
    const columns: any = [
      [
        'Views per Day',
        ...data.map(({ count }) => count)
      ]
    ];
    c3.generate({
      bindto: this.renderDiv.nativeElement,
      bar: {
        width: {
          ratio: 0.75
        }
      },
      size: {
        height: 500,
      },
      padding: {
        top: 50,
        bottom: 50,
        left: 50,
        right: 50
      },
      axis: {
        x: {
          tick: {
            format(d: number) {
              return 90 - d;
            }
          }
        }
      },
      data: {
        columns,
        type: 'bar'
      },
      tooltip: {
        show: true,
        format: {
          title: (x: number) => {
            return (data[x] && data[x].name) || "";
          }
        }
      }
    });
  }
}
