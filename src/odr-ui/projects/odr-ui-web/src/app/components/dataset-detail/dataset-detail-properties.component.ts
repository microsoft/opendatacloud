import {Component, Input} from '@angular/core';
import {Dataset} from '../../shared/types/dataset.type';

@Component({
  selector: 'app-dataset-detail-properties',
  templateUrl: './dataset-detail-properties.component.html',
  styleUrls: ['./dataset-detail-properties.component.scss']
})
export class DatasetDetailPropertiesComponent {
    @Input() dataset: Dataset;
}
