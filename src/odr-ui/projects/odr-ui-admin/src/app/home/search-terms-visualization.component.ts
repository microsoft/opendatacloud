// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import * as d3 from "d3";
import * as d3Colors from "d3-scale-chromatic";
import * as cloud from "d3-cloud";

@Component({
  selector: 'app-search-terms-visualization',
  template: '<div #renderDiv></div>'
})
export class SearchTermsVisualizationComponent implements OnChanges {
  @Input() data: any[];
  @ViewChild('renderDiv', { static: false }) renderDiv: ElementRef;

  ngOnChanges(changes: SimpleChanges): void {
    const { data } = changes;
    if (data && Array.isArray(data.currentValue)) {
      this.renderVisualization(data.currentValue);
    }
  }

  renderVisualization(rawData: any[]) {
    const { nativeElement } = this.renderDiv;
    const {width} = nativeElement.getBoundingClientRect();

    const maxCount = rawData.reduce((maxCount, {count}) => {
      return Math.max(count, maxCount);
    }, 0);

    const tagData = rawData.map(({name, count}) => ({
      text: name,
      size: count / maxCount * 100
    }));

    const colors = d3.scaleOrdinal(d3Colors.schemeTableau10);

    var layout = cloud()
      .size([width, Math.floor(width * 2 / 3)])
      .words(tagData)
      .padding(5)
      .rotate(() => (~~(Math.random() * 3) - 1.5) * 30)
      .font("Impact")
      .fontSize((d: any) => d.size)
      .on("end", (words: any) => {
        const [width, height] = layout.size();
        d3
          .select(nativeElement)
          .append("svg")
          .attr("width", width)
          .attr("height", height)
          .append("g")
          .attr("transform", `translate(${width/2},${height/2})`)
          .selectAll("text")
          .data(words)
          .enter()
            .append("text")
            .style("fill", (_, i) => colors(i.toString()) as string)
            .style("font-size", (d: any) => `${d.size}px`)
            .style("font-family", "Impact")
            .attr("text-anchor", "middle")
            .attr("transform", (d: any) => `translate(${[d.x, d.y]})rotate(${d.rotate})`)
            .text((d: any) => d.text);
      });

    layout.start();
  }
}
