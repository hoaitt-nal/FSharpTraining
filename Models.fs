namespace OrderProcessor

open System

type Product =
    { Id: int
      Name: string
      Price: decimal }

type OrderItem = { Product: Product; Quantity: int }

type Order =
    { Id: int
      Items: list<OrderItem>
      OrderDate: DateTime
      IsDiscounted: bool }