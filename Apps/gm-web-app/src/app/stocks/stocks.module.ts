import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MaterialModule } from '../material/material.module';
import { FuturesComponent } from './futures/futures.component';
import { StocksComponent } from './stocks.component';
import { StockRecommendationItemComponent } from './stock-recommendation-item/stock-recommendation-item.component';



@NgModule({
  declarations: [StocksComponent, FuturesComponent, StockRecommendationItemComponent],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule,
    MaterialModule
  ]
})
export class StocksModule { }
