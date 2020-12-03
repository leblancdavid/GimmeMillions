import { Component, Input, OnInit } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { StockRecommendation } from '../stock-recommendation';
import { RecommendationList } from './recommendation-list';

@Component({
  selector: 'gm-recommendation-list',
  templateUrl: './recommendation-list.component.html',
  styleUrls: ['./recommendation-list.component.scss']
})
export class RecommendationListComponent implements OnInit {

  @Input() recommendations!: RecommendationList;
  public selectedItem?: StockRecommendation;
  constructor() {
  }

  ngOnInit(): void {
  }

  select(r: StockRecommendation) {
    if (this.selectedItem && r.symbol == this.selectedItem.symbol) {
      this.selectedItem = undefined;
    } else {
      this.selectedItem = r;
    }
  }

  onFilterKeyup(event: Event) {
    this.recommendations.applyFilter((event.target as HTMLInputElement).value);
  }

  sortRecommendations(sort: Sort) {
    this.recommendations.applySort(sort);
  }
}

