import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
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
  @Input() selectedItem?: StockRecommendation;
  @Output() selectedItemChange = new EventEmitter<StockRecommendation>();

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
    
    this.selectedItemChange.emit(this.selectedItem);
  }


  sortRecommendations(sort: Sort) {
    this.recommendations.applySort(sort);
  }
}

