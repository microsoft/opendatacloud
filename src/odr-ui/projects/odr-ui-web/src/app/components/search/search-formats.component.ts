import { Component, EventEmitter, Input, Output } from "@angular/core";
import { Observable } from "rxjs";
import { OdrService } from "../../shared/services";
import { DatasetDomainType } from "../../shared/types";

// Formats selector
@Component({
  selector: "app-search-formats",
  styleUrls: ["./search-formats.component.scss"],
  templateUrl: "./search-formats.component.html"
})
export class SearchFormatsComponent {
  @Input() isCollapsed: boolean;
  @Input() tabindex: number;
  @Output() toggleCollapsed = new EventEmitter<boolean>();
  @Output() selectFormat = new EventEmitter<string>();

  formatsList: Observable<string[]>;

  constructor(private odrService: OdrService) {
    this.formatsList = this.odrService.getFileTypes();
  }

  onToggleClick() {
    this.toggleCollapsed.emit(!this.isCollapsed);
  }

  onFormatSelected(format: string) {
    this.selectFormat.emit(format);
  }
}
