import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StockRecommendation } from '../stock-recommendation';
import { RecommendationList } from './recommendation-list';

@Component({
  selector: 'gm-recommendation-list',
  templateUrl: './recommendation-list.component.html',
  styleUrls: ['./recommendation-list.component.scss']
})
export class RecommendationListComponent implements OnInit {

  @Input() recommendations!: RecommendationList;
  @Input() selectedItem?: StockRecommendation;
  @Output() selectedItemChange = new EventEmitter<StockRecommendation>();
  
  currentPageIndex: number;
  pageSize: number;
  currentPageStartIndex: number;
  currentPageEndIndex: number;
  constructor() {
    this.currentPageIndex = 0;
    this.pageSize = 25;
    this.currentPageStartIndex = 0;
    this.currentPageEndIndex = this.currentPageStartIndex + this.pageSize;
  }

  ngOnInit(): void {
  }

  select(r: StockRecommendation) {
    if (this.selectedItem && r.symbol == this.selectedItem.symbol) {
      this.selectedItem = undefined;
    } else {
      this.selectedItem = r;
    }
    
    this.selectedItemChange.emit(this.selectedItem);
  }

  sortRecommendations(sort: Sort) {
    this.recommendations.applySort(sort);
  }

  onPageChange(event: PageEvent) {
    this.currentPageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.currentPageStartIndex = this.currentPageIndex * this.pageSize;
    this.currentPageEndIndex = this.currentPageStartIndex + this.pageSize;
    this.selectedItem = undefined;
  }
}

