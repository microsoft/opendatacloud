// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component } from '@angular/core';
import {OdrService} from '../../shared/services/odr.service';
import {FAQType} from '../../shared/types/faq.type';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-faq',
  templateUrl: './faq.component.html',
  styleUrls: ['./faq.component.scss']
})
export class FaqComponent {

  faqs: Observable<FAQType[]>;

  constructor(
    private odrService: OdrService
  ) {
    this.faqs = this.odrService.getFAQs();
  }
}
