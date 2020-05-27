import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import * as c3 from 'c3';

@Component({
  selector: 'app-datasets-by-license-visualization',
  template: '<div #renderDiv></div>'
})
export class DatasetsByLicenseVisualizationComponent implements OnChanges {
  @Input() data: any[];
  @ViewChild('renderDiv', { static: false }) renderDiv: ElementRef;

  ngOnChanges(changes: SimpleChanges): void {
    const { data } = changes;
    if (data && Array.isArray(data.currentValue)) {
      this.renderVisualization(data.currentValue);
    }
  }

  renderVisualization(data: any[]) {
    const columns: any = data.map(({ name, count }) => [name, count]);
    c3.generate({
      bindto: this.renderDiv.nativeElement,
      data: {
        columns,
        type: 'pie',
        labels: true
      },
      legend: {
        position: 'bottom'
      }
    });
  }
}
